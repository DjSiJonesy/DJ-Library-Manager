using Avalonia.Media;
using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Services.Discovery;
using DJLibraryManager.UI.Services.Import;
using DJLibraryManager.UI.ViewModels.Workspace;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Dashboard;

public class DashboardWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title => "Dashboard";

    public ObservableCollection<WorkflowCardViewModel> WorkflowCards { get; } = new();

    public DashboardGuidanceViewModel Guidance { get; } = new();

    private readonly DashboardViewModel _dashboard;

    private readonly ImportValidationService
        _importValidationService = new();

    // ============================================================
    // Discovery
    // ============================================================

    public void UpdateDiscoveryStatus()
    {
        if (WorkflowCards.Count == 0)
            return;

        var discoverCard =
            WorkflowCards[0];

        var providerCount =
            _dashboard.InstalledProviders.Count(
                x => x.Installed);

        var mediaLocationCount =
            _dashboard.MediaLocations.Count;

        discoverCard.PrimaryStatisticTitle =
            "Providers Found";

        discoverCard.PrimaryStatisticValue =
            providerCount.ToString("N0");

        discoverCard.SecondaryStatisticTitle =
            "Media Locations";

        discoverCard.SecondaryStatisticValue =
            mediaLocationCount.ToString("N0");

        var discoverySessions =
            App.Services
                .DiscoveryRepository
                .DiscoverySessions;

        if (discoverySessions.Count < mediaLocationCount)
        {
            discoverCard.Status =
                "Ready to Discover";

            discoverCard.StatusBrush =
                Brushes.DeepSkyBlue;

            return;
        }

        discoverCard.Status =
            "Discovery Complete";

        discoverCard.StatusBrush =
            Brushes.LimeGreen;
    }

    // ============================================================
    // Import
    // ============================================================

    public async Task UpdateImportStatus()
    {
        if (WorkflowCards.Count < 2)
            return;

        var importCard =
            WorkflowCards[1];

        var importedProviders =
            _dashboard.InstalledProviders.Count(
                x =>
                    x.Installed &&
                    x.ImportState == ImportState.Imported);

        var repository =
            App.Services.LibraryRepository;

        try
        {
            var trackCount =
                await repository.GetTrackCountAsync();

            importCard.PrimaryStatisticTitle =
                "Tracks Imported";

            importCard.PrimaryStatisticValue =
                trackCount.ToString("N0");
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Unable to load SQLite track count: {exception}");
        }

        try
        {
            var playlistCount =
                await repository.GetPlaylistCountAsync();

            importCard.SecondaryStatisticTitle =
                "Playlists";

            importCard.SecondaryStatisticValue =
                playlistCount.ToString("N0");
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Unable to load playlist count: {exception}");

            importCard.SecondaryStatisticTitle =
                "Playlists";

            importCard.SecondaryStatisticValue =
                "0";
        }

        if (importedProviders == 0)
        {
            importCard.Status =
                "Ready to Import";

            importCard.StatusBrush =
                Brushes.DeepSkyBlue;

            return;
        }

        var discoverySessions =
            App.Services
                .DiscoveryRepository
                .DiscoverySessions;

        if (discoverySessions.Count <
            _dashboard.MediaLocations.Count)
        {
            importCard.Status =
                "Ready to Import";

            importCard.StatusBrush =
                Brushes.DeepSkyBlue;

            return;
        }

        var importRepository =
            App.Services.MediaImportRepository;

        foreach (var session in discoverySessions)
        {
            var importRecord =
                importRepository.Get(
                    session.MediaLocation.Path);

            if (importRecord is null)
            {
                importCard.Status =
                    "Ready to Import";

                importCard.StatusBrush =
                    Brushes.DeepSkyBlue;

                return;
            }

            if (_importValidationService.HasChanges(
                    session,
                    importRecord))
            {
                importCard.Status =
                    "Changes Detected";

                importCard.StatusBrush =
                    Brushes.Goldenrod;

                return;
            }
        }

        importCard.Status =
            "Import Complete";

        importCard.StatusBrush =
            Brushes.LimeGreen;
    }

    // ============================================================
    // Analysis
    // ============================================================

    public void UpdateAnalysisStatus()
    {
        if (WorkflowCards.Count < 3)
            return;

        var analysisCard =
            WorkflowCards[2];

        var analysis =
            App.Services
                .AnalysisRepository
                .CurrentAnalysis;

        if (analysis is null)
        {
            analysisCard.Status =
                "Ready";

            analysisCard.StatusBrush =
                Brushes.DeepSkyBlue;

            analysisCard.PrimaryStatisticTitle =
                "Health Score";

            analysisCard.PrimaryStatisticValue =
                "—";

            analysisCard.SecondaryStatisticTitle =
                "Issues Found";

            analysisCard.SecondaryStatisticValue =
                "—";

            return;
        }

        analysisCard.Status =
            "Analysis Complete";

        analysisCard.StatusBrush =
            Brushes.LimeGreen;

        analysisCard.PrimaryStatisticTitle =
            "Health Score";

        analysisCard.PrimaryStatisticValue =
            $"{analysis.HealthScore:F0}%";

        analysisCard.SecondaryStatisticTitle =
            "Issues Found";

        analysisCard.SecondaryStatisticValue =
            analysis.TotalIssues.ToString("N0");
    }

    // ============================================================
    // Search
    // ============================================================

    public void UpdateSearchStatus()
    {
        if (WorkflowCards.Count < 4)
            return;

        var searchCard =
            WorkflowCards[3];

        var analysis =
            App.Services
                .AnalysisRepository
                .CurrentAnalysis;

        if (analysis is null)
        {
            searchCard.Status =
                "Waiting";

            searchCard.StatusBrush =
                Brushes.Goldenrod;

            searchCard.PrimaryStatisticTitle =
                "Issues to Investigate";

            searchCard.PrimaryStatisticValue =
                "—";

            searchCard.SecondaryStatisticTitle =
                "Search Progress";

            searchCard.SecondaryStatisticValue =
                "—";

            return;
        }

        var totalIssues =
            analysis.TotalIssues;

        var searchState =
            App.Services
                .SearchRepository
                .CurrentSearch;

        var searchService =
            App.Services.Search;

        var currentRun =
            searchService.CurrentRun;

        if (searchService.IsSearching &&
            currentRun is not null)
        {
            searchCard.Status =
                "Searching";

            searchCard.StatusBrush =
                Brushes.DeepSkyBlue;

            searchCard.PrimaryStatisticTitle =
                "Issues to Investigate";

            searchCard.PrimaryStatisticValue =
                totalIssues.ToString("N0");

            searchCard.SecondaryStatisticTitle =
                "Search Progress";

            searchCard.SecondaryStatisticValue =
                $"{currentRun.IssuesSearched:N0} / " +
                $"{currentRun.TotalIssues:N0}";

            return;
        }

        if (searchState is null)
        {
            searchCard.Status =
                "Available";

            searchCard.StatusBrush =
                Brushes.DeepSkyBlue;

            searchCard.PrimaryStatisticTitle =
                "Issues to Investigate";

            searchCard.PrimaryStatisticValue =
                totalIssues.ToString("N0");

            searchCard.SecondaryStatisticTitle =
                "Search Progress";

            searchCard.SecondaryStatisticValue =
                "Not Started";

            return;
        }

        var searchedCount =
            searchState.Issues.Count(
                x => x.IsSearched);

        var issuesWithResults =
            searchState.Issues.Count(
                x => x.HasResults);

        if (searchedCount >= totalIssues &&
            totalIssues > 0)
        {
            searchCard.Status =
                "Search Complete";

            searchCard.StatusBrush =
                Brushes.LimeGreen;

            searchCard.PrimaryStatisticTitle =
                "Issues Investigated";

            searchCard.PrimaryStatisticValue =
                searchedCount.ToString("N0");

            searchCard.SecondaryStatisticTitle =
                "Issues with Results";

            searchCard.SecondaryStatisticValue =
                issuesWithResults.ToString("N0");

            return;
        }

        searchCard.Status =
            "Search Available";

        searchCard.StatusBrush =
            Brushes.DeepSkyBlue;

        searchCard.PrimaryStatisticTitle =
            "Issues to Investigate";

        searchCard.PrimaryStatisticValue =
            totalIssues.ToString("N0");

        searchCard.SecondaryStatisticTitle =
            "Search Progress";

        searchCard.SecondaryStatisticValue =
            $"{searchedCount:N0} / {totalIssues:N0}";
    }

    // ============================================================
    // Search Progress
    // ============================================================

    private void OnSearchProgressChanged(
        object? sender,
        SearchRun run)
    {
        UpdateSearchStatus();
    }

    // ============================================================
    // Constructor
    // ============================================================

    public DashboardWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard =
            dashboard;

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Discovery,

                HoverAction =
                    stage => Guidance.Show(stage),

                ActionCommand =
                    _dashboard.OpenDiscoveryCommand,

                PrimaryStatisticTitle =
                    "Providers Found",

                PrimaryStatisticValue =
                    string.Empty,

                SecondaryStatisticTitle =
                    "Media Locations",

                SecondaryStatisticValue =
                    string.Empty
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Import,

                HoverAction =
                    stage => Guidance.Show(stage),

                ActionCommand =
                    _dashboard.OpenImportCommand,

                Status =
                    "Ready to Import",

                StatusBrush =
                    Brushes.DeepSkyBlue,

                PrimaryStatisticTitle =
                    "Tracks Imported",

                PrimaryStatisticValue =
                    "0",

                SecondaryStatisticTitle =
                    "Playlists",

                SecondaryStatisticValue =
                    "0"
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Analysis,

                HoverAction =
                    stage => Guidance.Show(stage),

                ActionCommand =
                    _dashboard.OpenAnalysisCommand,

                Status =
                    "Ready",

                StatusBrush =
                    Brushes.DeepSkyBlue,

                PrimaryStatisticTitle =
                    "Health Score",

                PrimaryStatisticValue =
                    "—",

                SecondaryStatisticTitle =
                    "Issues Found",

                SecondaryStatisticValue =
                    "—"
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Search,

                HoverAction =
                    stage => Guidance.Show(stage),

                ActionCommand =
                    _dashboard.OpenSearchCommand,

                Status =
                    "Available",

                StatusBrush =
                    Brushes.DeepSkyBlue,

                PrimaryStatisticTitle =
                    "Issues to Investigate",

                PrimaryStatisticValue =
                    "—",

                SecondaryStatisticTitle =
                    "Search Progress",

                SecondaryStatisticValue =
                    "Not Started"
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Improve,

                HoverAction =
                    stage => Guidance.Show(stage),

                Status =
                    "Waiting",

                StatusBrush =
                    Brushes.Goldenrod,

                PrimaryStatisticTitle =
                    "Suggestions",

                PrimaryStatisticValue =
                    "412",

                SecondaryStatisticTitle =
                    "Auto Fixes",

                SecondaryStatisticValue =
                    "97"
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Structure,

                HoverAction =
                    stage => Guidance.Show(stage),

                Status =
                    "Waiting",

                StatusBrush =
                    Brushes.Goldenrod,

                PrimaryStatisticTitle =
                    "Recommendations",

                PrimaryStatisticValue =
                    "126",

                SecondaryStatisticTitle =
                    "Approved",

                SecondaryStatisticValue =
                    "0"
            });

        WorkflowCards.Add(
            new WorkflowCardViewModel
            {
                Definition =
                    WorkflowDefinitions.Synchronise,

                HoverAction =
                    stage => Guidance.Show(stage),

                Status =
                    "Waiting",

                StatusBrush =
                    Brushes.Goldenrod,

                PrimaryStatisticTitle =
                    "Pending Changes",

                PrimaryStatisticValue =
                    "0",

                SecondaryStatisticTitle =
                    "Ready",

                SecondaryStatisticValue =
                    "No"
            });

        UpdateDiscoveryStatus();

        _ = UpdateImportStatus();

        UpdateAnalysisStatus();

        UpdateSearchStatus();

        App.Services.Search.ProgressChanged +=
            OnSearchProgressChanged;

        Guidance.Reset();
    }
}