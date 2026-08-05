using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.ViewModels.Workspace;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels;

public class DiscoveryWorkspaceViewModel : WorkspaceViewModel
{
    private readonly DashboardViewModel _dashboard;

    public override string Title => "Discovery";

    /// <summary>
    /// Installed DJ providers only.
    /// </summary>
    public IEnumerable<ProviderInfo> InstalledProviders =>
        _dashboard.InstalledProviders
                  .Where(provider => provider.Installed);

    public DiscoveryWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard = dashboard;
    }
}