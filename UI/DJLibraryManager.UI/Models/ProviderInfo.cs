using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Input;

namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents the current state of a provider library import.
/// </summary>
public enum ImportState
{
    NotImported,
    Importing,
    Imported,
    Failed
}

/// <summary>
/// Represents a DJ software provider displayed by the UI.
/// </summary>
public partial class ProviderInfo : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool installed;

    [ObservableProperty]
    private string version = string.Empty;

    [ObservableProperty]
    private string? installPath;

    [ObservableProperty]
    private string? executablePath;

    [ObservableProperty]
    private string? databasePath;

    [ObservableProperty]
    private string? settingsPath;

    [ObservableProperty]
    private Bitmap? providerLogo;

    [ObservableProperty]
    private ICommand? openCommand;

    // --------------------------------------------------------------------
    // Import State
    // --------------------------------------------------------------------

    [ObservableProperty]
    private ImportState importState = ImportState.NotImported;

    // Temporary compatibility property.
    // This will be removed once the ViewModels have been migrated.
    public bool LibraryImported
    {
        get => ImportState == ImportState.Imported;
        set
        {
            ImportState = value
                ? ImportState.Imported
                : ImportState.NotImported;
        }
    }

    [ObservableProperty]
    private DateTime? lastImported;

    [ObservableProperty]
    private int trackCount;

    [ObservableProperty]
    private int playlistCount;

    /// <summary>
    /// Indicates whether an import is currently running.
    /// </summary>
    public bool IsImporting => ImportState == ImportState.Importing;

    /// <summary>
    /// Indicates whether the provider can be imported.
    /// </summary>
    public bool CanImport => ImportState != ImportState.Importing;

    /// <summary>
    /// Indicates whether library analysis can be performed.
    /// </summary>
    public bool CanAnalyse => ImportState == ImportState.Imported;

    /// <summary>
    /// Friendly import status.
    /// </summary>
    public string LibraryStatus =>
        ImportState switch
        {
            ImportState.NotImported => "Not Imported",
            ImportState.Importing => "Importing...",
            ImportState.Imported => "✓ Imported",
            ImportState.Failed => "Import Failed",
            _ => "Unknown"
        };

    /// <summary>
    /// Opacity used when displaying the provider card.
    /// </summary>
    public double CardOpacity => Installed ? 1.0 : 0.45;

    /// <summary>
    /// Cursor displayed when hovering over the provider card.
    /// </summary>
    public Cursor CardCursor =>
        Installed
            ? new Cursor(StandardCursorType.Hand)
            : new Cursor(StandardCursorType.Arrow);

    /// <summary>
    /// Indicates whether hover effects are enabled.
    /// </summary>
    public bool CanHover => Installed;

    /// <summary>
    /// Friendly installation status.
    /// </summary>
    public string Status =>
        Installed
            ? "✓ Installed"
            : "Not Installed";

    partial void OnInstalledChanged(bool value)
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CardOpacity));
        OnPropertyChanged(nameof(CardCursor));
        OnPropertyChanged(nameof(CanHover));
    }

    partial void OnImportStateChanged(ImportState value)
    {
        OnPropertyChanged(nameof(LibraryImported));
        OnPropertyChanged(nameof(LibraryStatus));
        OnPropertyChanged(nameof(IsImporting));
        OnPropertyChanged(nameof(CanImport));
        OnPropertyChanged(nameof(CanAnalyse));
    }
}