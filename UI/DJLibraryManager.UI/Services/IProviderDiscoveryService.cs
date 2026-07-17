using DJLibraryManager.UI.Models;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Services;

public interface IProviderDiscoveryService
{
    IEnumerable<ProviderDiscoveryResult> DiscoverProviders();
}