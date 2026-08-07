using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Models.Discovery;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services.Import;
using DJLibraryManager.UI.ViewModels.Workspace;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Import;

public partial class ImportWorkspaceViewModel : WorkspaceViewModel
{
    private readonly DashboardViewModel _dashboard;
    private readonly MediaImportService _mediaImportService;

    public override string Title => "Import";

    public ImportWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard = dashboard;

        _mediaImportService = new MediaImportService(
            App.Services.LibraryRepository);
    }

    /// <summary>
    /// Installed providers only.
    /// </summary>
    public IEnumerable<ProviderInfo> InstalledProviders =>
        _dashboard.InstalledProviders
                  .Where(x => x.Installed);

    public int ProviderCount =>
        InstalledProviders.Count();

    public int TotalTracks =>
    InstalledProviders.Sum(provider => provider.TrackCount);

    public int TotalPlaylists =>
        InstalledProviders.Sum(provider => provider.PlaylistCount);

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
    /// Return to Discovery.
    /// </summary>
    [RelayCommand]
    private void GoDiscovery()
    {
        App.Services.ApplicationState.NavigateTo(
            WorkspaceType.Discovery);
    }

    /// <summary>
    /// Opens the selected media location.
    /// </summary>
    [RelayCommand]
    private void ViewMediaLocation(
        MediaLocationDiscoverySummary summary)
    {
        if (summary is null)
            return;

        _dashboard.SelectMediaLocationCommand.Execute(
            summary.MediaLocation);
    }

    /// <summary>
    /// Imports a provider library.
    /// </summary>
    [RelayCommand]
    private async Task ImportProvider(
        ProviderInfo provider)
    {
        if (provider is null)
            return;

        await App.Services
            .LibraryImportService
            .ImportAsync(provider);

        OnPropertyChanged(nameof(InstalledProviders));
    }

    /// <summary>
    /// Imports all media beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private async Task ImportMedia(
        MediaLocationDiscoverySummary summary)
    {
        if (summary is null)
            return;

        await _mediaImportService.ImportAsync(
            new[] { summary.MediaLocation });

        OnPropertyChanged(nameof(MediaLocations));
        OnPropertyChanged(nameof(TotalDrives));
        OnPropertyChanged(nameof(TotalFolders));
        OnPropertyChanged(nameof(TotalAudioFiles));
        OnPropertyChanged(nameof(TotalVideoFiles));

        _dashboard.LibraryOverview.Refresh();
        _dashboard.DashboardWorkspace?.UpdateImportStatus();
    }
}