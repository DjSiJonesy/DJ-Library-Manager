using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<string> InstalledProviders { get; } = new()
    {
        "VirtualDJ",
        "Rekordbox"
    };

    public ObservableCollection<string> MusicLocations { get; } = new()
    {
        @"C:\Users\Simon\Music",
        @"D:\Music"
    };

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusText = "Ready";
}