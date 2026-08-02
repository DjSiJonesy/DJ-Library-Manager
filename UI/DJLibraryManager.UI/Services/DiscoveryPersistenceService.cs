using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Persists discovery sessions between application launches.
/// </summary>
public sealed class DiscoveryPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _applicationFolder;
    private readonly string _discoveryFile;

    public DiscoveryPersistenceService()
    {
        ApplicationPaths.EnsureCreated();

        _applicationFolder = ApplicationPaths.Root;
        _discoveryFile = ApplicationPaths.DiscoverySessions;
    }

    /// <summary>
    /// Loads all previously saved discovery sessions.
    /// </summary>
    public IReadOnlyList<DiscoverySession> Load()
    {
        try
        {
            if (!File.Exists(_discoveryFile))
            {
                return Array.Empty<DiscoverySession>();
            }

            var json = File.ReadAllText(_discoveryFile);

            var sessions =
                JsonSerializer.Deserialize<List<DiscoverySession>>(
                    json,
                    JsonOptions);

            return sessions ?? new List<DiscoverySession>();
        }
        catch
        {
            // Corrupt or unreadable discovery file.
            // Start with an empty repository rather than
            // preventing the application from loading.
            return Array.Empty<DiscoverySession>();
        }
    }

    /// <summary>
    /// Saves all discovery sessions.
    /// </summary>
    public void Save(IEnumerable<DiscoverySession> sessions)
    {
        Directory.CreateDirectory(_applicationFolder);

        var json = JsonSerializer.Serialize(
            sessions,
            JsonOptions);

        File.WriteAllText(
            _discoveryFile,
            json);
    }
}