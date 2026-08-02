using System.Collections.Generic;
using System.Linq;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Builds the information displayed by the Library Explorer.
/// This service combines information from discovery and, in future,
/// import, analysis, search and synchronisation.
/// </summary>
public sealed class ExplorerService
{
    private readonly MediaLocationRepository _mediaLocationRepository;
    private readonly DiscoveryRepository _discoveryRepository;

    public ExplorerService(
        MediaLocationRepository mediaLocationRepository,
        DiscoveryRepository discoveryRepository)
    {
        _mediaLocationRepository = mediaLocationRepository;
        _discoveryRepository = discoveryRepository;
    }

    /// <summary>
    /// Builds the explorer items for all known media locations.
    /// </summary>
    public IReadOnlyList<MediaLocationExplorerItem> BuildExplorer()
    {
        return _mediaLocationRepository.MediaLocations
            .Select(location => new MediaLocationExplorerItem
            {
                MediaLocation = location,
                DiscoverySession = _discoveryRepository.Get(location.Path)
            })
            .OrderBy(item => item.MediaLocation.Name)
            .ToList();
    }
}