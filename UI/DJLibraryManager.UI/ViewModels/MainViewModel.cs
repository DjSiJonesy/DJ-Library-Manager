using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.Models;
using System.Collections.ObjectModel;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<ProviderInfo> InstalledProviders { get; } = new()
{
    new ProviderInfo
    {
        Name = "VirtualDJ",
        Installed = true,
        Version = ""
    },

    new ProviderInfo
    {
        Name = "Rekordbox",
        Installed = true,
        Version = ""
    }
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