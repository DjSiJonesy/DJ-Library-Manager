using System;
using System.Collections.Generic;
using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Services;

namespace DJLibraryManager.UI.Services.Discovery;

/// <summary>
/// Performs discovery for a single media location.
/// This service is shared by the Discovery workflow and the
/// Media Location workspace.
/// </summary>
public sealed class MediaDiscoveryService
{
    private readonly MediaLibraryDiscoveryService _libraryDiscovery = new();
    private readonly DiscoveryRepository _repository = App.Services.DiscoveryRepository;

    public DiscoverySession Discover(MediaLocation mediaLocation)
    {
        ArgumentNullException.ThrowIfNull(mediaLocation);

        var libraries = _libraryDiscovery.DiscoverLibraries(mediaLocation);

        var session = new DiscoverySession
        {
            MediaLocation = mediaLocation,
            Libraries = libraries,
            DiscoveryDate = DateTime.Now
        };

        _repository.Save(session);

        return session;
    }
}