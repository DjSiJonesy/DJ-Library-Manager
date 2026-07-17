using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Providers.Detection;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Services;

public class ProviderDiscoveryService : IProviderDiscoveryService
{
    private readonly IProviderDetector[] _detectors =
    [
        new VirtualDJDetector(),
        new RekordboxDetector(),
        new SeratoDetector(),
        new EngineDJDetector(),
        new TraktorDetector()
    ];

    public IEnumerable<ProviderDiscoveryResult> DiscoverProviders()
    {
        return _detectors.Select(d => d.Discover());
    }
}