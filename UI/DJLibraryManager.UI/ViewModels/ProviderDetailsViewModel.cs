using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.ViewModels;

public partial class ProviderDetailsViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboard;

    public ProviderInfo Provider { get; }

    public ProviderDetailsViewModel(
        ProviderInfo provider,
        DashboardViewModel dashboard)
    {
        Provider = provider;
        _dashboard = dashboard;
    }

    public string ProviderName => Provider.Name;

    public bool IsInstalled => Provider.Installed;

    public string InstalledText =>
        Provider.Installed
            ? "✓ Installed"
            : "Not Installed";

    public string Version =>
        string.IsNullOrWhiteSpace(Provider.Version)
            ? "Unknown"
            : Provider.Version;

    public string ExecutablePath =>
        string.IsNullOrWhiteSpace(Provider.ExecutablePath)
            ? "Not Available"
            : Provider.ExecutablePath;

    public string DatabasePath =>
        string.IsNullOrWhiteSpace(Provider.DatabasePath)
            ? "Not Available"
            : Provider.DatabasePath;

    public string SettingsPath =>
        string.IsNullOrWhiteSpace(Provider.SettingsPath)
            ? "Not Available"
            : Provider.SettingsPath;

    public Avalonia.Media.Imaging.Bitmap? ProviderLogo =>
        Provider.ProviderLogo;

    public event EventHandler? GoBackRequested;

    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenInstallationFolder()
    {
        if (string.IsNullOrWhiteSpace(Provider.ExecutablePath))
        {
            return;
        }

        var folder = Path.GetDirectoryName(Provider.ExecutablePath);

        FolderLauncher.Open(folder);
    }

    [RelayCommand]
    private void OpenDatabaseFolder()
    {
        Console.WriteLine($"DatabasePath = '{Provider.DatabasePath}'");

        FolderLauncher.Open(Provider.DatabasePath);
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        FolderLauncher.Open(Provider.SettingsPath);
    }
}