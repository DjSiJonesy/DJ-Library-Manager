using CommunityToolkit.Mvvm.ComponentModel;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// The application's dashboard.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

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

        // Display the dashboard when the application starts.
        CurrentView = Dashboard;

        // Listen for navigation requests from the dashboard.
        Dashboard.ProviderSelected += Dashboard_ProviderSelected;
    }

    private void Dashboard_ProviderSelected(object? sender, ProviderSelectedEventArgs e)
    {
        CurrentView = new ProviderDetailsViewModel(e.Provider, Dashboard);
        StatusText = $"Viewing {e.Provider.Name}";
    }
}