using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Models.Discovery;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Operations;
using DJLibraryManager.UI.Services.Import;
using DJLibraryManager.UI.ViewModels.Workspace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            App.Services.ProgressReporter,
            App.Services.LibraryRepository);

        LoadMediaLocations();

        App.Services.ProgressReporter.CurrentOperation.PropertyChanged +=
            CurrentOperation_PropertyChanged;
    }

    /// <summary>
    /// Current application operation.
    /// </summary>
    public OperationProgress CurrentOperation =>
        App.Services.ProgressReporter.CurrentOperation;

    private void CurrentOperation_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentOperation));
    }

    // ============================================================
    // Provider Import
    // ============================================================

    public IEnumerable<ProviderInfo> InstalledProviders =>
        _dashboard.InstalledProviders
                  .Where(x => x.Installed);

    public int ProviderCount =>
        InstalledProviders.Count();

    public int TotalTracks =>
        InstalledProviders.Sum(provider => provider.TrackCount);

    public int TotalPlaylists =>
        InstalledProviders.Sum(provider => provider.PlaylistCount);

    // ============================================================
    // Media Import
    // ============================================================

    public ObservableCollection<MediaLocationImportInfo> MediaLocations { get; }
        = new();

    private void LoadMediaLocations()
    {
        MediaLocations.Clear();

        foreach (var summary in App.Services
                     .DiscoveryRepository
                     .GetSummaries(_dashboard.MediaLocations))
        {
            var mediaLocation = new MediaLocationImportInfo
            {
                Summary = summary
            };

            var record = App.Services.MediaImportRepository.Get(
                summary.MediaLocation.Path);

            if (record is not null)
            {
                mediaLocation.ImportState = record.ImportState;
                mediaLocation.LastImported = record.LastImported;
            }

            MediaLocations.Add(mediaLocation);
        }

        OnPropertyChanged(nameof(TotalDrives));
        OnPropertyChanged(nameof(TotalFolders));
        OnPropertyChanged(nameof(TotalAudioFiles));
        OnPropertyChanged(nameof(TotalVideoFiles));
    }

    public int TotalDrives =>
        MediaLocations.Count;

    public int TotalFolders =>
        MediaLocations.Sum(x => x.FolderCount);

    /// <summary>
    /// Total media files (Audio + Video).
    /// </summary>
    public int TotalAudioFiles =>
        MediaLocations.Sum(x => x.TotalMediaFiles);

    public int TotalVideoFiles =>
        MediaLocations.Sum(x => x.Summary.VideoFileCount);

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
        MediaLocationImportInfo mediaLocation)
    {
        if (mediaLocation is null)
            return;

        _dashboard.SelectMediaLocationCommand.Execute(
            mediaLocation.Summary.MediaLocation);
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

        if (provider.IsImporting)
            return;

        provider.ImportState = ImportState.Importing;

        try
        {
            var result = await App.Services
                .LibraryImportService
                .ImportAsync(provider);

            if (!result.Success)
            {
                provider.ImportState = ImportState.Failed;
                return;
            }

            provider.LastImported = result.ImportedAt;
            provider.TrackCount = result.TrackCount;
            provider.PlaylistCount = result.PlaylistCount;
            provider.ImportState = ImportState.Imported;

            OnPropertyChanged(nameof(InstalledProviders));
            OnPropertyChanged(nameof(ProviderCount));
            OnPropertyChanged(nameof(TotalTracks));
            OnPropertyChanged(nameof(TotalPlaylists));

            _dashboard.LibraryOverview.Refresh();
            _dashboard.DashboardWorkspace?.UpdateImportStatus();
        }
        catch
        {
            provider.ImportState = ImportState.Failed;
            throw;
        }
    }

    /// <summary>
    /// Imports all media beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private async Task ImportMedia(
        MediaLocationImportInfo mediaLocation)
    {
        if (mediaLocation is null)
            return;

        if (mediaLocation.IsImporting)
            return;

        mediaLocation.ImportState = MediaImportState.Importing;

        try
        {
            var result = await _mediaImportService.ImportAsync(
                new[] { mediaLocation.Summary.MediaLocation });

            mediaLocation.LastImported = DateTime.Now;
            mediaLocation.ImportState = MediaImportState.Imported;

            App.Services.MediaImportRepository.Save(
                new MediaImportRecord
                {
                    LocationPath = mediaLocation.Path,
                    ImportState = mediaLocation.ImportState,
                    LastImported = mediaLocation.LastImported,
                    ImportedFiles = result.Imported,
                    SkippedFiles = result.Skipped,
                    FailedFiles = result.Failed
                });

            System.Diagnostics.Debug.WriteLine(
                $"Media Import - " +
                $"Scanned={result.Scanned}, " +
                $"Imported={result.Imported}, " +
                $"Skipped={result.Skipped}, " +
                $"Failed={result.Failed}");
        }
        catch
        {
            mediaLocation.ImportState = MediaImportState.Failed;
            throw;
        }

        OnPropertyChanged(nameof(MediaLocations));
        OnPropertyChanged(nameof(TotalDrives));
        OnPropertyChanged(nameof(TotalFolders));
        OnPropertyChanged(nameof(TotalAudioFiles));
        OnPropertyChanged(nameof(TotalVideoFiles));

        _dashboard.LibraryOverview.Refresh();
        _dashboard.DashboardWorkspace?.UpdateImportStatus();
    }
}