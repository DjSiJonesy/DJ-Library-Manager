using System;
using System.Collections.Generic;
using System.Linq;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Stores the media locations known to DIASISS DJ.
/// This repository is populated during application startup and
/// acts as the single source of truth for all discovered media
/// locations.
/// </summary>
public sealed class MediaLocationRepository
{
    private readonly Dictionary<string, MediaLocation> _mediaLocations =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Indicates whether any media locations are stored.
    /// </summary>
    public bool HasMediaLocations => _mediaLocations.Count > 0;

    /// <summary>
    /// Returns all known media locations.
    /// </summary>
    public IReadOnlyCollection<MediaLocation> MediaLocations =>
        _mediaLocations.Values.ToList().AsReadOnly();

    /// <summary>
    /// Stores or replaces a media location.
    /// </summary>
    public void Save(MediaLocation mediaLocation)
    {
        ArgumentNullException.ThrowIfNull(mediaLocation);

        _mediaLocations[mediaLocation.Path] = mediaLocation;
    }

    /// <summary>
    /// Stores or replaces multiple media locations.
    /// </summary>
    public void Save(IEnumerable<MediaLocation> mediaLocations)
    {
        ArgumentNullException.ThrowIfNull(mediaLocations);

        foreach (var mediaLocation in mediaLocations)
        {
            Save(mediaLocation);
        }
    }

    /// <summary>
    /// Returns true if the specified media location exists.
    /// </summary>
    public bool Contains(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return _mediaLocations.ContainsKey(path);
    }

    /// <summary>
    /// Retrieves a media location by its path.
    /// Returns null if it does not exist.
    /// </summary>
    public MediaLocation? Get(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return _mediaLocations.TryGetValue(path, out var mediaLocation)
            ? mediaLocation
            : null;
    }

    /// <summary>
    /// Removes a media location.
    /// </summary>
    public void Remove(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _mediaLocations.Remove(path);
    }

    /// <summary>
    /// Clears all stored media locations.
    /// </summary>
    public void Clear()
    {
        _mediaLocations.Clear();
    }
}