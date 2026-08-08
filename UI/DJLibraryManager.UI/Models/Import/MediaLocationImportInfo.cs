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

    public int TotalMediaFiles => Summary.TotalMediaFiles;

    [ObservableProperty]
    private MediaImportState importState = MediaImportState.Ready;

    [ObservableProperty]
    private DateTime? lastImported;

    public bool IsImporting =>
        ImportState == MediaImportState.Importing;

    public bool CanImport =>
        ImportState != MediaImportState.Importing;

    public string ImportStatus =>
        ImportState switch
        {
            MediaImportState.Ready => "Ready to Import",
            MediaImportState.Importing => "Importing...",
            MediaImportState.Imported => "Imported",
            MediaImportState.Failed => "Import Failed",
            _ => "Unknown"
        };

    public string ImportActionText =>
        ImportState == MediaImportState.Imported
            ? "Re-import"
            : "Import";

    public IBrush ImportStatusBrush =>
        ImportState switch
        {
            MediaImportState.Ready => Brushes.Orange,
            MediaImportState.Importing => Brushes.DeepSkyBlue,
            MediaImportState.Imported => Brushes.LimeGreen,
            MediaImportState.Failed => Brushes.Red,
            _ => Brushes.Gray
        };

    partial void OnImportStateChanged(MediaImportState value)
    {
        OnPropertyChanged(nameof(IsImporting));
        OnPropertyChanged(nameof(CanImport));
        OnPropertyChanged(nameof(ImportStatus));
        OnPropertyChanged(nameof(ImportActionText));
        OnPropertyChanged(nameof(ImportStatusBrush));
    }
}