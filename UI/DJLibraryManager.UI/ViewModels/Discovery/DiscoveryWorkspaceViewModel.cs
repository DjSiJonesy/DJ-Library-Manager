using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Discovery;
using DJLibraryManager.UI.Services.Discovery;
using DJLibraryManager.UI.ViewModels.Workspace;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels;

public partial class DiscoveryWorkspaceViewModel : WorkspaceViewModel
{
    private readonly DashboardViewModel _dashboard;
    private readonly MediaDiscoveryService _mediaDiscoveryService = new();
    private readonly DiscoveryValidationService _validationService = new();

    public override string Title => "Discovery";

    public DiscoveryWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard = dashboard;

        LoadMediaLocations();
    }

    /// <summary>
    /// Installed DJ providers only.
    /// </summary>
    public IEnumerable<ProviderInfo> InstalledProviders =>
        _dashboard.InstalledProviders
                  .Where(provider => provider.Installed);

    // ============================================================
    // Media Discovery
    // ============================================================

    public ObservableCollection<MediaLocationDiscoveryInfo> MediaLocations { get; }
        = new();

    private void LoadMediaLocations()
    {
        MediaLocations.Clear();

        foreach (var summary in App.Services
                     .DiscoveryRepository
                     .GetSummaries(_dashboard.MediaLocations))
        {
            var info = new MediaLocationDiscoveryInfo
            {
                Summary = summary
            };

            var session =
                App.Services.DiscoveryRepository.Get(
                    summary.MediaLocation.Path);

            if (session is not null)
            {
                var validation =
                    App.Services
                        .DiscoveryValidationRepository
                        .Get(session.MediaLocation.Path);

                info.HasChanges =
                    validation?.HasChanges ?? false;
            }

            MediaLocations.Add(info);
        }

        OnPropertyChanged(nameof(TotalDrives));
        OnPropertyChanged(nameof(TotalFolders));
        OnPropertyChanged(nameof(TotalAudioFiles));
        OnPropertyChanged(nameof(TotalVideoFiles));
    }

    /// <summary>
    /// Reloads the Discovery workspace from the cached repositories.
    /// </summary>
    public void Refresh()
    {
        LoadMediaLocations();
    }

    public int TotalDrives =>
        MediaLocations.Count;

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
    private void ViewMediaLocation(
        MediaLocationDiscoveryInfo mediaLocation)
    {
        if (mediaLocation is null)
            return;

        _dashboard.SelectMediaLocationCommand.Execute(
            mediaLocation.Summary.MediaLocation);
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
    /// Validates every discovered media location and updates
    /// the cached Discovery validation results.
    /// </summary>
    [RelayCommand]
    private void ValidateDiscovery()
    {
        App.Services
            .DiscoveryValidationWorkflowService
            .Validate(
                App.Services
                    .DiscoveryRepository
                    .DiscoverySessions);

        Refresh();
    }

    /// <summary>
    /// Discovers media beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private void DiscoverMedia(
        MediaLocationDiscoveryInfo mediaLocation)
    {
        if (mediaLocation is null)
            return;

        //
        // Perform Discovery.
        //

        _mediaDiscoveryService.Discover(
            mediaLocation.Summary.MediaLocation);

        //
        // Retrieve the newly updated Discovery Session.
        //

        var session =
            App.Services
                .DiscoveryRepository
                .Get(mediaLocation.Summary.MediaLocation.Path);

        //
        // Refresh the cached validation for this location.
        //

        if (session is not null)
        {
            App.Services
                .DiscoveryValidationWorkflowService
                .Validate(session);
        }

        //
        // Refresh the Discovery workspace.
        //

        Refresh();
    }
}