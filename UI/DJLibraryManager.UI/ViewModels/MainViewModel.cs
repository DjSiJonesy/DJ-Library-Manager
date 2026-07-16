using System;
using System.Collections.ObjectModel;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<ProviderInfo> InstalledProviders { get; } = new()
    {
        new ProviderInfo
        {
            Name = "VirtualDJ",
            Installed = true,
            Version = "",
            ProviderLogo = new Bitmap(
                AssetLoader.Open(
                    new Uri("avares://DJLibraryManager.UI/Assets/Providers/VirtualDJ.png")))
        },

        new ProviderInfo
        {
            Name = "Rekordbox",
            Installed = true,
            Version = "",
            ProviderLogo = new Bitmap(
                AssetLoader.Open(
                    new Uri("avares://DJLibraryManager.UI/Assets/Providers/Rekordbox.png")))
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