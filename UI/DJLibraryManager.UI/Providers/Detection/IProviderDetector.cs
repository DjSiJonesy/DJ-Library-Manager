using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Defines the contract for detecting a supported DJ provider.
/// </summary>
public interface IProviderDetector
{
    ProviderDiscoveryResult Discover();
}