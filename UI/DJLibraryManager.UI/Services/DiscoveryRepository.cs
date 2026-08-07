using System;
using System.Collections.Generic;
using System.Linq;

using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Models.Discovery;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Stores discovery sessions for media locations.
/// Each media location maintains its own discovery state,
/// allowing multiple locations to be discovered independently.
/// </summary>
public sealed class DiscoveryRepository
{
    private readonly Dictionary<string, DiscoverySession> _discoveries =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly DiscoveryPersistenceService _persistenceService;
    private readonly ApplicationState _applicationState;

    public DiscoveryRepository(ApplicationState applicationState)
    {
        ArgumentNullException.ThrowIfNull(applicationState);

        _applicationState = applicationState;
        _persistenceService = new DiscoveryPersistenceService();

        // Restore any previously saved discovery sessions.
        foreach (var session in _persistenceService.Load())
        {
            _discoveries[session.MediaLocation.Path] = session;
        }
    }

    /// <summary>
    /// Indicates whether any discovery sessions exist.
    /// </summary>
    public bool HasDiscoveries => _discoveries.Count > 0;

    /// <summary>
    /// Returns all discovery sessions currently held by the repository.
    /// </summary>
    public IReadOnlyCollection<DiscoverySession> DiscoverySessions =>
        _discoveries.Values.ToList().AsReadOnly();

    /// <summary>
    /// Stores or replaces a discovery session.
    /// </summary>
    public void Save(DiscoverySession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _discoveries[session.MediaLocation.Path] = session;

        Persist();

        _applicationState.NotifyDiscoveryChanged();
    }

    /// <summary>
    /// Returns true if a discovery exists for the specified media location.
    /// </summary>
    public bool HasDiscovery(string mediaLocationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaLocationPath);

        return _discoveries.ContainsKey(mediaLocationPath);
    }

    /// <summary>
    /// Retrieves a discovery session for the specified media location.
    /// Returns null if no discovery has been performed.
    /// </summary>
    public DiscoverySession? Get(string mediaLocationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaLocationPath);

        return _discoveries.TryGetValue(mediaLocationPath, out var session)
            ? session
            : null;
    }

    /// <summary>
    /// Returns a discovery summary for the specified media location.
    /// If no discovery has been performed a default summary is returned.
    /// </summary>
    public MediaLocationDiscoverySummary GetSummary(MediaLocation mediaLocation)
    {
        ArgumentNullException.ThrowIfNull(mediaLocation);

        if (!_discoveries.TryGetValue(mediaLocation.Path, out var session))
        {
            return new MediaLocationDiscoverySummary
            {
                MediaLocation = mediaLocation,
                DiscoveryDate = null,
                FolderCount = 0,
                AudioFileCount = 0,
                VideoFileCount = 0,
                TotalSizeBytes = 0,
                Status = mediaLocation.Exists
                    ? "Ready to Discover"
                    : "Location Not Available"
            };
        }

        return new MediaLocationDiscoverySummary
        {
            MediaLocation = mediaLocation,
            DiscoveryDate = session.DiscoveryDate,
            FolderCount = session.Libraries.Count,
            AudioFileCount = session.Libraries.Sum(x => x.AudioFileCount),
            VideoFileCount = session.Libraries.Sum(x => x.VideoFileCount),
            TotalSizeBytes = session.Libraries.Sum(x => x.TotalSizeBytes),
            Status = "Discovery Complete"
        };
    }

    /// <summary>
    /// Returns discovery summaries for every known media location.
    /// </summary>
    public IReadOnlyCollection<MediaLocationDiscoverySummary> GetSummaries(
        IEnumerable<MediaLocation> mediaLocations)
    {
        ArgumentNullException.ThrowIfNull(mediaLocations);

        return mediaLocations
            .Select(GetSummary)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Removes the discovery for a single media location.
    /// </summary>
    public void Remove(string mediaLocationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaLocationPath);

        if (_discoveries.Remove(mediaLocationPath))
        {
            Persist();

            _applicationState.NotifyDiscoveryChanged();
        }
    }

    /// <summary>
    /// Clears all stored discovery sessions.
    /// </summary>
    public void Clear()
    {
        _discoveries.Clear();

        Persist();

        _applicationState.NotifyDiscoveryChanged();
    }

    /// <summary>
    /// Persists the current discovery sessions.
    /// </summary>
    private void Persist()
    {
        _persistenceService.Save(_discoveries.Values);
    }
}