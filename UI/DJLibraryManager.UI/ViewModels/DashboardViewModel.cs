using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Dashboard displayed when the application starts.
/// Responsible for discovering installed providers.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string> ProviderLogos = new()
    {
        ["VirtualDJ"] = "VirtualDJ.png",
        ["Rekordbox"] = "Rekordbox2.png",
        ["Serato"] = "Serato4.png",
        ["Traktor"] = "Traktor.jpeg",
        ["EngineDJ"] = "EngineDJ.png"
    };

    /// <summary>
    /// Raised when the user selects a provider.
    /// During the transition to the workspace architecture this
    /// is still used by MainViewModel.
    /// </summary>
    public event EventHandler<ProviderSelectedEventArgs>? ProviderSelected;

    public ObservableCollection<ProviderInfo> InstalledProviders { get; } = new();

    public ObservableCollection<string> MusicLocations { get; } = new()
    {
        @"C:\Users\Simon\Music",
        @"D:\Music"
    };

    [ObservableProperty]
    private string? selectedProviderName;

    [ObservableProperty]
    private ProviderInfo? selectedProvider;

    public DashboardViewModel()
    {
        var discoveryService = new ProviderDiscoveryService();

        foreach (var provider in discoveryService.DiscoverProviders())
        {
            InstalledProviders.Add(CreateProvider(provider));
        }
    }

    [RelayCommand]
    private void SelectProvider(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return;

        var provider = InstalledProviders.FirstOrDefault(
            p => p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
            return;

        if (!provider.Installed)
            return;

        SelectedProvider = provider;
        SelectedProviderName = provider.Name;

        ProviderSelected?.Invoke(
            this,
            new ProviderSelectedEventArgs(provider));
    }

    private ProviderInfo CreateProvider(ProviderDiscoveryResult provider)
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
            InstallPath = provider.InstallPath,
            ExecutablePath = provider.ExecutablePath,
            DatabasePath = provider.DatabasePath,
            SettingsPath = provider.SettingsPath,
            ProviderLogo = logo,
            OpenCommand = SelectProviderCommand
        };
    }
}

/// <summary>
/// Event arguments raised when a provider is selected.
/// </summary>
public sealed class ProviderSelectedEventArgs : EventArgs
{
    public ProviderInfo Provider { get; }

    public ProviderSelectedEventArgs(ProviderInfo provider)
    {
        Provider = provider;
    }
}