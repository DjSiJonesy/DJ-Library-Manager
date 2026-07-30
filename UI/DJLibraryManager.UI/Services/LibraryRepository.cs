using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Library;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Stores and retrieves the application's provider-independent media library.
/// </summary>
public sealed class LibraryRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Root folder used by DJ Library Manager.
    /// </summary>
    private readonly string _applicationFolder;

    /// <summary>
    /// Location of the persisted media library.
    /// </summary>
    private readonly string _libraryFile;

    /// <summary>
    /// Location of the persisted library metadata.
    /// </summary>
    private readonly string _metadataFile;

    public LibraryRepository()
    {
        _applicationFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DJLibraryManager");

        _libraryFile = Path.Combine(
            _applicationFolder,
            "MediaLibrary.json");

        _metadataFile = Path.Combine(
            _applicationFolder,
            "LibraryMetadata.json");
    }

    /// <summary>
    /// Returns true if a persisted library already exists.
    /// </summary>
    public bool Exists()
    {
        return File.Exists(_libraryFile);
    }

    /// <summary>
    /// Returns true if the specified provider has an imported library.
    /// </summary>
    public async Task<bool> ProviderLibraryExistsAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        var library = await LoadAsync();

        return library.Any(item =>
            item.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the number of tracks imported for the specified provider.
    /// </summary>
    public async Task<int> GetProviderTrackCountAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        var library = await LoadAsync();

        return library.Count(item =>
            item.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Saves (or replaces) the imported library for a single provider.
    /// Existing media from the same provider is removed before the new
    /// media is added, preserving libraries imported from other providers.
    /// </summary>
    public async Task SaveProviderLibraryAsync(
        string providerName,
        IEnumerable<DJLMMediaItem> mediaItems)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);
        ArgumentNullException.ThrowIfNull(mediaItems);

        Directory.CreateDirectory(_applicationFolder);

        var library = (await LoadAsync()).ToList();

        // Remove any existing media from this provider.
        library.RemoveAll(item =>
            item.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        // Add the newly imported media.
        library.AddRange(mediaItems);

        await using var stream = File.Create(_libraryFile);

        await JsonSerializer.SerializeAsync(
            stream,
            library,
            JsonOptions);
    }

    /// <summary>
    /// Loads the persisted media library.
    /// </summary>
    public async Task<IReadOnlyList<DJLMMediaItem>> LoadAsync()
    {
        if (!Exists())
            return Array.Empty<DJLMMediaItem>();

        await using var stream = File.OpenRead(_libraryFile);

        var mediaItems =
            await JsonSerializer.DeserializeAsync<List<DJLMMediaItem>>(
                stream,
                JsonOptions);

        return mediaItems ?? new List<DJLMMediaItem>();
    }

    /// <summary>
    /// Returns all media belonging to a specific provider.
    /// </summary>
    public async Task<IReadOnlyList<DJLMMediaItem>> LoadProviderLibraryAsync(
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        var library = await LoadAsync();

        return library
            .Where(item =>
                item.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Saves the import metadata for a provider.
    /// </summary>
    public async Task SaveProviderImportAsync(
        ProviderInfo provider,
        ImportResult result)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(result);

        Directory.CreateDirectory(_applicationFolder);

        var metadata = await LoadProviderMetadataAsync();

        metadata.RemoveAll(p =>
            p.ProviderName.Equals(provider.Name, StringComparison.OrdinalIgnoreCase));

        metadata.Add(new ProviderLibraryMetadata
        {
            ProviderName = provider.Name,
            LastImported = result.ImportedAt,
            TrackCount = result.TrackCount,
            PlaylistCount = result.PlaylistCount
        });

        await using var stream = File.Create(_metadataFile);

        await JsonSerializer.SerializeAsync(
            stream,
            metadata,
            JsonOptions);
    }

    /// <summary>
    /// Returns the stored import metadata for a provider.
    /// </summary>
    public async Task<ProviderLibraryMetadata?> GetProviderImportAsync(
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        var metadata = await LoadProviderMetadataAsync();

        return metadata.FirstOrDefault(p =>
            p.ProviderName.Equals(providerName,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Loads the provider metadata.
    /// </summary>
    private async Task<List<ProviderLibraryMetadata>> LoadProviderMetadataAsync()
    {
        if (!File.Exists(_metadataFile))
            return new List<ProviderLibraryMetadata>();

        await using var stream = File.OpenRead(_metadataFile);

        var metadata =
            await JsonSerializer.DeserializeAsync<List<ProviderLibraryMetadata>>(
                stream,
                JsonOptions);

        return metadata ?? new List<ProviderLibraryMetadata>();
    }

    /// <summary>
    /// Removes all imported media.
    /// </summary>
    public Task ClearAsync()
    {
        if (File.Exists(_libraryFile))
        {
            File.Delete(_libraryFile);
        }

        if (File.Exists(_metadataFile))
        {
            File.Delete(_metadataFile);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the imported library for a specific provider.
    /// </summary>
    public async Task ClearProviderLibraryAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        var library = (await LoadAsync())
            .Where(item =>
                !item.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Directory.CreateDirectory(_applicationFolder);

        await using var stream = File.Create(_libraryFile);

        await JsonSerializer.SerializeAsync(
            stream,
            library,
            JsonOptions);

        var metadata = await LoadProviderMetadataAsync();

        metadata.RemoveAll(p =>
            p.ProviderName.Equals(providerName,
                StringComparison.OrdinalIgnoreCase));

        await using var metadataStream = File.Create(_metadataFile);

        await JsonSerializer.SerializeAsync(
            metadataStream,
            metadata,
            JsonOptions);
    }
}