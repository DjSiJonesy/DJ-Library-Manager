using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.ViewModels.Dashboard;
using DJLibraryManager.UI.ViewModels.Import;
using DJLibraryManager.UI.ViewModels.Analysis;
using DJLibraryManager.UI.ViewModels.Search;
using DJLibraryManager.UI.ViewModels.Improve;

using DJLibraryManager.UI.ViewModels.Library;
using DJLibraryManager.UI.ViewModels.Workspace;
using System;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// The application's dashboard.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// The currently imported library.
    /// </summary>
    public ImportResult? CurrentLibrary { get; private set; }

    /// <summary>
    /// The currently displayed workspace.
    /// </summary>
    [ObservableProperty]
    private WorkspaceViewModel? currentWorkspace;

    /// <summary>
    /// Status text displayed in the footer.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Ready";

    public MainViewModel()
    {
        Dashboard = new DashboardViewModel();

        var workspace =
            new DashboardWorkspaceViewModel(Dashboard);

        CurrentWorkspace = workspace;

        Dashboard.CurrentWorkspace = workspace;
        Dashboard.DashboardWorkspace = workspace;

        Dashboard.ProviderSelected +=
            Dashboard_ProviderSelected;

        Dashboard.MediaLocationSelected +=
            Dashboard_MediaLocationSelected;

        Dashboard.LibraryExplorerSelected +=
            Dashboard_LibraryExplorerSelected;

        App.Services.ApplicationState.NavigateRequested +=
            ApplicationState_NavigateRequested;
    }

    // ============================================================
    // Provider Workspace
    // ============================================================

    /// <summary>
    /// Opens the selected provider workspace.
    /// </summary>
    private void Dashboard_ProviderSelected(
        object? sender,
        ProviderSelectedEventArgs e)
    {
        var workspace = new ProviderWorkspaceViewModel(
            e.Provider,
            Dashboard,
            LibraryImported);

        workspace.GoBackRequested +=
            Workspace_GoBackRequested;

        CurrentWorkspace = workspace;
        Dashboard.CurrentWorkspace = workspace;

        StatusText =
            $"Viewing {e.Provider.Name}";
    }

    // ============================================================
    // Media Location Workspace
    // ============================================================

    /// <summary>
    /// Opens the selected media location workspace.
    /// </summary>
    private void Dashboard_MediaLocationSelected(
        object? sender,
        MediaLocationSelectedEventArgs e)
    {
        var workspace =
            new MediaLocationWorkspaceViewModel(
                e.MediaLocation);

        workspace.GoBackRequested +=
            Workspace_GoBackRequested;

        CurrentWorkspace = workspace;
        Dashboard.CurrentWorkspace = workspace;

        StatusText =
            $"Viewing {e.MediaLocation.Path}";
    }

    // ============================================================
    // Library Explorer
    // ============================================================

    /// <summary>
    /// Opens the Library Explorer workspace.
    /// </summary>
    private void Dashboard_LibraryExplorerSelected(
        object? sender,
        EventArgs e)
    {
        var workspace =
            new LibraryExplorerViewModel();

        CurrentWorkspace = workspace;
        Dashboard.CurrentWorkspace = workspace;

        StatusText =
            "Viewing Library Explorer";
    }

    // ============================================================
    // Application Navigation
    // ============================================================

    /// <summary>
    /// Handles application-wide workspace navigation requests.
    /// </summary>
    private void ApplicationState_NavigateRequested(
        object? sender,
        WorkspaceType workspace)
    {
        switch (workspace)
        {
            // ====================================================
            // Dashboard
            // ====================================================

            case WorkspaceType.Dashboard:
                {
                    var dashboardWorkspace =
                        new DashboardWorkspaceViewModel(Dashboard);

                    CurrentWorkspace = dashboardWorkspace;

                    Dashboard.CurrentWorkspace =
                        dashboardWorkspace;

                    Dashboard.DashboardWorkspace =
                        dashboardWorkspace;

                    StatusText = "Ready";

                    break;
                }

            // ====================================================
            // Discovery
            // ====================================================

            case WorkspaceType.Discovery:
                {
                    var discoveryWorkspace =
                        new DiscoveryWorkspaceViewModel(Dashboard);

                    CurrentWorkspace =
                        discoveryWorkspace;

                    Dashboard.CurrentWorkspace =
                        discoveryWorkspace;

                    StatusText =
                        "Viewing Discovery";

                    break;
                }

            // ====================================================
            // Import
            // ====================================================

            case WorkspaceType.Import:
                {
                    var importWorkspace =
                        new ImportWorkspaceViewModel(Dashboard);

                    CurrentWorkspace =
                        importWorkspace;

                    Dashboard.CurrentWorkspace =
                        importWorkspace;

                    StatusText =
                        "Viewing Import";

                    break;
                }

            // ====================================================
            // Analysis
            // ====================================================

            case WorkspaceType.Analysis:
                {
                    var analysisWorkspace =
                        new AnalysisWorkspaceViewModel();

                    CurrentWorkspace =
                        analysisWorkspace;

                    Dashboard.CurrentWorkspace =
                        analysisWorkspace;

                    StatusText =
                        "Viewing Analysis";

                    break;
                }

            // ====================================================
            // Search
            // ====================================================

            case WorkspaceType.Search:
                {
                    var searchWorkspace =
                        new SearchWorkspaceViewModel();

                    CurrentWorkspace =
                        searchWorkspace;

                    Dashboard.CurrentWorkspace =
                        searchWorkspace;

                    StatusText =
                        "Viewing Search";

                    break;
                }

            // ====================================================
            // Improve
            // ====================================================

            case WorkspaceType.Improve:
                {
                    var improveWorkspace =
                        new ImproveWorkspaceViewModel();

                    CurrentWorkspace =
                        improveWorkspace;

                    Dashboard.CurrentWorkspace =
                        improveWorkspace;

                    StatusText =
                        "Viewing Improve";

                    break;
                }
        }
    }

    // ============================================================
    // Import
    // ============================================================

    /// <summary>
    /// Called when a provider finishes importing its library.
    /// </summary>
    private void LibraryImported(
        ImportResult result)
    {
        CurrentLibrary = result;

        StatusText =
            $"Imported {result.TrackCount:N0} tracks from {result.ProviderName}";
    }

    // ============================================================
    // Dashboard Return
    // ============================================================

    /// <summary>
    /// Returns to the dashboard.
    /// </summary>
    private void Workspace_GoBackRequested(
        object? sender,
        EventArgs e)
    {
        var workspace =
            new DashboardWorkspaceViewModel(Dashboard);

        CurrentWorkspace = workspace;

        Dashboard.CurrentWorkspace =
            workspace;

        Dashboard.DashboardWorkspace =
            workspace;

        StatusText =
            "Ready";
    }
}