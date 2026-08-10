using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.Core.Models.Discovery;

using System;

namespace DJLibraryManager.UI.Models.Import;

public enum MediaImportState
{
    Ready,
    Importing,
    Imported,
    Failed
}

public partial class MediaLocationImportInfo : ObservableObject
{
    /// <summary>
    /// Discovery summary this import entry represents.
    /// </summary>
    public required MediaLocationDiscoverySummary Summary { get; init; }

    public string Path => Summary.MediaLocation.Path;

    public int FolderCount => Summary.FolderCount;

    /// <summary>
    /// Total media files currently discovered.
    /// </summary>
    public int TotalMediaFiles => Summary.TotalMediaFiles;

    // ============================================================
    // Persisted Import Statistics
    // ============================================================

    /// <summary>
    /// Number of files imported during the last import.
    /// </summary>
    [ObservableProperty]
    private int importedFiles;

    /// <summary>
    /// Number of files skipped because they already existed.
    /// </summary>
    [ObservableProperty]
    private int skippedFiles;

    /// <summary>
    /// Number of files that failed during import.
    /// </summary>
    [ObservableProperty]
    private int failedFiles;

    [ObservableProperty]
    private MediaImportState importState = MediaImportState.Ready;

    [ObservableProperty]
    private DateTime? lastImported;

    /// <summary>
    /// Discovery date used for the last successful import.
    /// </summary>
    [ObservableProperty]
    private DateTime? lastDiscoveryDate;

    /// <summary>
    /// Total number of files discovered during the last successful import.
    /// </summary>
    [ObservableProperty]
    private int importedTotalFiles;

    // ============================================================
    // Calculated Statistics
    // ============================================================

    /// <summary>
    /// Current discovered files.
    /// </summary>
    public int DiscoveredFiles =>
        Summary.TotalMediaFiles;

    /// <summary>
    /// Files already present in the DIASISS Library.
    /// </summary>
    public int AlreadyInLibrary =>
        SkippedFiles;

    /// <summary>
    /// Indicates whether an import is currently running.
    /// </summary>
    public bool IsImporting =>
        ImportState == MediaImportState.Importing;

    /// <summary>
    /// Indicates whether this location can currently be imported.
    /// </summary>
    public bool CanImport =>
        ImportState != MediaImportState.Importing;

    /// <summary>
    /// True when Discovery has found changes since the last import.
    /// </summary>
    public bool HasChanges =>
        ImportState == MediaImportState.Imported &&
        Summary.TotalMediaFiles != ImportedTotalFiles;

    /// <summary>
    /// Friendly import status.
    /// </summary>
    public string ImportStatus =>
        ImportState switch
        {
            MediaImportState.Ready => "Ready to Import",
            MediaImportState.Importing => "Importing...",
            MediaImportState.Failed => "Import Failed",

            MediaImportState.Imported when HasChanges
                => "Changes Detected",

            MediaImportState.Imported
                => "Fully Imported",

            _ => "Unknown"
        };

    /// <summary>
    /// Friendly action button text.
    /// </summary>
    public string ImportActionText =>
        ImportState switch
        {
            MediaImportState.Imported when HasChanges
                => "Import Changes",

            MediaImportState.Imported
                => "Re-import",

            _ => "Import"
        };

    /// <summary>
    /// Status indicator colour.
    /// </summary>
    public IBrush ImportStatusBrush =>
        ImportState switch
        {
            MediaImportState.Ready => Brushes.Orange,
            MediaImportState.Importing => Brushes.DeepSkyBlue,

            MediaImportState.Imported when HasChanges
                => Brushes.Goldenrod,

            MediaImportState.Imported
                => Brushes.LimeGreen,

            MediaImportState.Failed => Brushes.Red,

            _ => Brushes.Gray
        };

    partial void OnImportStateChanged(MediaImportState value)
    {
        OnPropertyChanged(nameof(IsImporting));
        OnPropertyChanged(nameof(CanImport));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(ImportStatus));
        OnPropertyChanged(nameof(ImportActionText));
        OnPropertyChanged(nameof(ImportStatusBrush));
    }

    partial void OnLastDiscoveryDateChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(ImportStatus));
        OnPropertyChanged(nameof(ImportActionText));
        OnPropertyChanged(nameof(ImportStatusBrush));
    }

    partial void OnImportedTotalFilesChanged(int value)
    {
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(ImportStatus));
        OnPropertyChanged(nameof(ImportActionText));
        OnPropertyChanged(nameof(ImportStatusBrush));
    }
}