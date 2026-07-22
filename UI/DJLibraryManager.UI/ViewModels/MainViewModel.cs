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
    /// The view currently displayed by the MainWindow.
    /// 
    /// NOTE:
    /// During the transition to the new workspace architecture the
    /// Dashboard remains the application's home view. Selecting a
    /// provider still opens the Provider Workspace.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase currentView;

    /// <summary>
    /// Status text displayed in the footer.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Ready";

    public MainViewModel()
    {
        Dashboard = new DashboardViewModel();

        CurrentView = Dashboard;

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

        CurrentView = workspace;

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
        CurrentView = Dashboard;

        StatusText = "Ready";
    }
}