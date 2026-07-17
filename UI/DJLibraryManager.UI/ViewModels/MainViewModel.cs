using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string> ProviderLogos = new()
    {
        ["VirtualDJ"] = "VirtualDJ.png",
        ["Rekordbox"] = "Rekordbox2.png",
        ["Serato"] = "Serato4.png",
        ["Traktor"] = "Traktor.jpeg",
        ["EngineDJ"] = "EngineDJ.png"
    };

    public ObservableCollection<ProviderInfo> InstalledProviders { get; } = new();

    public ObservableCollection<string> MusicLocations { get; } = new()
    {
        @"C:\Users\Simon\Music",
        @"D:\Music"
    };

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusText = "Ready";

    public MainViewModel()
    {
        var discoveryService = new ProviderDiscoveryService();

        foreach (var provider in discoveryService.DiscoverProviders())
        {
            InstalledProviders.Add(CreateProvider(provider));
        }
    }

    private static ProviderInfo CreateProvider(ProviderDiscoveryResult provider)
    {
        ProviderLogos.TryGetValue(provider.Name, out var logoFile);

        Bitmap? logo = null;

        if (!string.IsNullOrWhiteSpace(logoFile))
        {
            logo = new Bitmap(
                AssetLoader.Open(
                    new Uri($"avares://DJLibraryManager.UI/Assets/Providers/{logoFile}")));
        }

        return new ProviderInfo
        {
            Name = provider.Name,
            Installed = provider.Installed,
            Version = provider.Version,
            ProviderLogo = logo
        };
    }
}