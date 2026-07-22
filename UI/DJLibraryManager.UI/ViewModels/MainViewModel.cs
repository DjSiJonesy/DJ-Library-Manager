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

    private void Dashboard_ProviderSelected(object? sender, ProviderSelectedEventArgs e)
    {
        var details = new ProviderDetailsViewModel(
            e.Provider,
            Dashboard,
            LibraryImported);

        details.GoBackRequested += Details_GoBackRequested;

        CurrentView = details;

        StatusText = $"Viewing {e.Provider.Name}";
    }

    private void LibraryImported(ImportResult result)
    {
        CurrentLibrary = result;

        StatusText =
            $"Imported {result.TrackCount:N0} tracks from {result.ProviderName}";
    }

    private void Details_GoBackRequested(object? sender, EventArgs e)
    {
        CurrentView = Dashboard;
    }
}