using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.Models.Import;
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
    /// The currently displayed provider workspace.
    /// Null when no provider has been selected.
    /// </summary>
    [ObservableProperty]
    private ProviderWorkspaceViewModel? currentWorkspace;

    /// <summary>
    /// Status text displayed in the footer.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Ready";

    public MainViewModel()
    {
        Dashboard = new DashboardViewModel();

        CurrentWorkspace = null;
        Dashboard.CurrentWorkspace = null;

        Dashboard.ProviderSelected += Dashboard_ProviderSelected;
    }

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

        workspace.GoBackRequested += Workspace_GoBackRequested;

        CurrentWorkspace = workspace;
        Dashboard.CurrentWorkspace = workspace;

        StatusText = $"Viewing {e.Provider.Name}";
    }

    /// <summary>
    /// Called when a provider finishes importing its library.
    /// </summary>
    private void LibraryImported(ImportResult result)
    {
        CurrentLibrary = result;

        StatusText =
            $"Imported {result.TrackCount:N0} tracks from {result.ProviderName}";
    }

    /// <summary>
    /// Returns to the dashboard.
    /// </summary>
    private void Workspace_GoBackRequested(
        object? sender,
        EventArgs e)
    {
        CurrentWorkspace = null;
        Dashboard.CurrentWorkspace = null;

        StatusText = "Ready";
    }
}