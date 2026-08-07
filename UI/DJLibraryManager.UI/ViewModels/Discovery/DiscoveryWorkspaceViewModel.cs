using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Models.Discovery;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services.Discovery;
using DJLibraryManager.UI.ViewModels.Workspace;

using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels;

public partial class DiscoveryWorkspaceViewModel : WorkspaceViewModel
{
    private readonly DashboardViewModel _dashboard;
    private readonly MediaDiscoveryService _mediaDiscoveryService = new();

    public override string Title => "Discovery";

    public DiscoveryWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>
    /// Installed DJ providers only.
    /// </summary>
    public IEnumerable<ProviderInfo> InstalledProviders =>
        _dashboard.InstalledProviders
                  .Where(provider => provider.Installed);

    /// <summary>
    /// Discovery summaries for each media location.
    /// </summary>
    public IEnumerable<MediaLocationDiscoverySummary> MediaLocations =>
        App.Services.DiscoveryRepository
            .GetSummaries(_dashboard.MediaLocations);

    public int TotalDrives =>
        MediaLocations.Count();

    public int TotalFolders =>
        MediaLocations.Sum(x => x.FolderCount);

    public int TotalAudioFiles =>
        MediaLocations.Sum(x => x.AudioFileCount);

    public int TotalVideoFiles =>
        MediaLocations.Sum(x => x.VideoFileCount);

   
    /// <summary>
    /// Opens the selected media location.
    /// </summary>
    [RelayCommand]
    private void ViewMediaLocation(MediaLocationDiscoverySummary summary)
    {
        if (summary is null)
            return;

        _dashboard.SelectMediaLocationCommand.Execute(summary.MediaLocation);
    }

    /// <summary>
    /// Navigates to the Import workspace.
    /// </summary>
    [RelayCommand]
    private void GoImport()
    {
        App.Services.ApplicationState.NavigateTo(
            WorkspaceType.Import);
    }

    /// <summary>
    /// Discovers media beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private void DiscoverMedia(MediaLocationDiscoverySummary summary)
    {
        if (summary is null)
            return;

        _mediaDiscoveryService.Discover(summary.MediaLocation);

        OnPropertyChanged(nameof(MediaLocations));
        OnPropertyChanged(nameof(TotalDrives));
        OnPropertyChanged(nameof(TotalFolders));
        OnPropertyChanged(nameof(TotalAudioFiles));
        OnPropertyChanged(nameof(TotalVideoFiles));
    }
}