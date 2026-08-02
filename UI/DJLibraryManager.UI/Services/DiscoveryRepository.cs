using System;
using System.Collections.Generic;
using System.Linq;

using DJLibraryManager.Core.Models;

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

    public DiscoveryRepository()
    {
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
        _discoveries[session.MediaLocation.Path] = session;

        Persist();
    }

    /// <summary>
    /// Returns true if a discovery exists for the specified media location.
    /// </summary>
    public bool HasDiscovery(string mediaLocationPath)
    {
        return _discoveries.ContainsKey(mediaLocationPath);
    }

    /// <summary>
    /// Retrieves a discovery session for the specified media location.
    /// Returns null if no discovery has been performed.
    /// </summary>
    public DiscoverySession? Get(string mediaLocationPath)
    {
        return _discoveries.TryGetValue(mediaLocationPath, out var session)
            ? session
            : null;
    }

    /// <summary>
    /// Removes the discovery for a single media location.
    /// </summary>
    public void Remove(string mediaLocationPath)
    {
        if (_discoveries.Remove(mediaLocationPath))
        {
            Persist();
        }
    }

    /// <summary>
    /// Clears all stored discovery sessions.
    /// </summary>
    public void Clear()
    {
        _discoveries.Clear();

        Persist();
    }

    /// <summary>
    /// Persists the current discovery sessions.
    /// </summary>
    private void Persist()
    {
        _persistenceService.Save(_discoveries.Values);
    }
}