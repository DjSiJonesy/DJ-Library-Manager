using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Operations;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.ViewModels.Workspace;

namespace DJLibraryManager.UI.ViewModels;

public partial class ProviderWorkspaceViewModel : WorkspaceViewModel, IDisposable
{
    private readonly DashboardViewModel _dashboard;
    private readonly Action<ImportResult> _libraryImported;

    public ProviderInfo Provider { get; }
    public override string Title => Provider.Name;

    public ProviderWorkspaceViewModel(
        ProviderInfo provider,
        DashboardViewModel dashboard,
        Action<ImportResult> libraryImported)
    {
        Provider = provider;
        _dashboard = dashboard;
        _libraryImported = libraryImported;

        Provider.PropertyChanged += Provider_PropertyChanged;

        App.Services.ProgressReporter.CurrentOperation.PropertyChanged +=
            CurrentOperation_PropertyChanged;
    }

    public string ProviderName => Provider.Name;

    public bool IsInstalled => Provider.Installed;

    public IBrush StatusBrush => Provider.StatusBrush;

    public string StatusText => Provider.Status;

    public string InstalledText =>
        Provider.Installed
            ? "✓ Installed"
            : "Not Installed";

    public bool LibraryImported => Provider.LibraryImported;

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

    public Bitmap? ProviderLogo => Provider.ProviderLogo;

    /// <summary>
    /// The currently running application operation.
    /// </summary>
    public OperationProgress CurrentOperation =>
        App.Services.ProgressReporter.CurrentOperation;

    public event EventHandler? GoBackRequested;

    [RelayCommand]
    private async Task ImportLibraryAsync()
    {
        if (Provider.IsImporting)
            return;

        Provider.ImportState = ImportState.Importing;

        try
        {
            var result = await App.Services.LibraryImportService.ImportAsync(Provider);

            if (!result.Success)
            {
                Provider.ImportState = ImportState.Failed;
                return;
            }

            Provider.LastImported = result.ImportedAt;
            Provider.TrackCount = result.TrackCount;
            Provider.PlaylistCount = result.PlaylistCount;

            Provider.ImportState = ImportState.Imported;

            _libraryImported(result);
        }
        catch
        {
            Provider.ImportState = ImportState.Failed;
            throw;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenInstallationFolder()
    {
        if (string.IsNullOrWhiteSpace(Provider.ExecutablePath))
            return;

        FolderLauncher.Open(Path.GetDirectoryName(Provider.ExecutablePath));
    }

    [RelayCommand]
    private void OpenDatabaseFolder()
    {
        FolderLauncher.Open(Provider.DatabasePath);
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        FolderLauncher.Open(Provider.SettingsPath);
    }

    private void Provider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ProviderInfo.Installed):
                OnPropertyChanged(nameof(IsInstalled));
                OnPropertyChanged(nameof(StatusBrush));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(InstalledText));
                break;

            case nameof(ProviderInfo.StatusBrush):
                OnPropertyChanged(nameof(StatusBrush));
                break;

            case nameof(ProviderInfo.Status):
                OnPropertyChanged(nameof(StatusText));
                break;

            case nameof(ProviderInfo.LibraryImported):
                OnPropertyChanged(nameof(LibraryImported));
                break;

            case nameof(ProviderInfo.Name):
                OnPropertyChanged(nameof(ProviderName));
                break;

            case nameof(ProviderInfo.Version):
                OnPropertyChanged(nameof(Version));
                break;

            case nameof(ProviderInfo.ExecutablePath):
                OnPropertyChanged(nameof(ExecutablePath));
                break;

            case nameof(ProviderInfo.DatabasePath):
                OnPropertyChanged(nameof(DatabasePath));
                break;

            case nameof(ProviderInfo.SettingsPath):
                OnPropertyChanged(nameof(SettingsPath));
                break;

            case nameof(ProviderInfo.ProviderLogo):
                OnPropertyChanged(nameof(ProviderLogo));
                break;
        }
    }

    private void CurrentOperation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentOperation));
    }

    public void Dispose()
    {
        Provider.PropertyChanged -= Provider_PropertyChanged;

        App.Services.ProgressReporter.CurrentOperation.PropertyChanged -=
            CurrentOperation_PropertyChanged;
    }
}