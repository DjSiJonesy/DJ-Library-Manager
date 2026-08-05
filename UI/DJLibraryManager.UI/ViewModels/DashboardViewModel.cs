using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.ViewModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Dashboard displayed when the application starts.
/// Responsible for discovering installed providers and media locations.
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

    public event EventHandler<ProviderSelectedEventArgs>? ProviderSelected;
    public event EventHandler<MediaLocationSelectedEventArgs>? MediaLocationSelected;
    public event EventHandler? LibraryExplorerSelected;

    public ObservableCollection<ProviderInfo> InstalledProviders { get; } = new();

    public ObservableCollection<MediaLocation> MediaLocations { get; } = new();

    public LibraryOverviewViewModel LibraryOverview { get; } = new();

    /// <summary>
    /// The Dashboard workspace currently being displayed.
    /// Used to update workflow cards as discovery progresses.
    /// </summary>
    public DashboardWorkspaceViewModel? DashboardWorkspace { get; set; }

    [ObservableProperty]
    private WorkspaceViewModel? currentWorkspace;

    [ObservableProperty]
    private string? selectedProviderName;

    [ObservableProperty]
    private ProviderInfo? selectedProvider;

    public DashboardViewModel()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadProvidersAsync();

        LoadMediaLocations();

        DashboardWorkspace?.UpdateDiscoveryStatus();
        DashboardWorkspace?.UpdateImportStatus();
    }

    private async Task LoadProvidersAsync()
    {
        var discoveryService = new ProviderDiscoveryService();
        var repository = App.Services.LibraryRepository;

        foreach (var discoveredProvider in discoveryService.DiscoverProviders())
        {
            var provider = CreateProvider(discoveredProvider);

            var metadata =
        await repository.GetProviderImportAsync(provider.Name);

            if (provider.Installed && metadata is not null)
            {
                provider.ImportState = ImportState.Imported;
                provider.TrackCount = metadata.TrackCount;
                provider.PlaylistCount = metadata.PlaylistCount;
                provider.LastImported = metadata.LastImported;
            }

            InstalledProviders.Add(provider);
            DashboardWorkspace?.UpdateImportStatus();
        }
    }

    private void LoadMediaLocations()
    {
        var discoveryService = new MediaLocationDiscoveryService();
        var repository = App.Services.MediaLocationRepository;

        // Refresh the repository.
        repository.Clear();
        repository.Save(discoveryService.DiscoverLocations());

        // Populate the Dashboard from the repository.
        MediaLocations.Clear();

        foreach (var location in repository.MediaLocations.OrderBy(x => x.Name))
        {
            MediaLocations.Add(location);
        }
    }

    [RelayCommand]
    private void SelectProvider(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return;

        var provider = InstalledProviders.FirstOrDefault(
            p => p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null || !provider.Installed)
            return;

        SelectedProvider = provider;
        SelectedProviderName = provider.Name;

        ProviderSelected?.Invoke(
            this,
            new ProviderSelectedEventArgs(provider));
    }

    [RelayCommand]
    private void OpenMediaLocation(MediaLocation? location)
    {
        if (location is null)
            return;

        FolderLauncher.Open(location.Path);
    }

    [RelayCommand]
    private void SelectMediaLocation(MediaLocation? location)
    {
        if (location is null)
            return;

        MediaLocationSelected?.Invoke(
            this,
            new MediaLocationSelectedEventArgs(location));
    }

    [RelayCommand]
    private void OpenLibraryExplorer()
    {
        LibraryExplorerSelected?.Invoke(this, EventArgs.Empty);
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
/// <summary>
/// Event arguments raised when a media location is selected.
/// </summary>
public sealed class MediaLocationSelectedEventArgs : EventArgs
{
    public MediaLocation MediaLocation { get; }

    public MediaLocationSelectedEventArgs(MediaLocation mediaLocation)
    {
        MediaLocation = mediaLocation;
    }
}