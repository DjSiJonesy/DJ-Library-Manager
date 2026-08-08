using System.Collections.Generic;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models.Import;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaLocationImportTable : UserControl
{
    public WorkflowMediaLocationImportTable()
    {
        InitializeComponent();
    }

    // ============================================================
    // Media Locations
    // ============================================================

    public static readonly StyledProperty<IEnumerable<MediaLocationImportInfo>?> MediaLocationsProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, IEnumerable<MediaLocationImportInfo>?>(
            nameof(MediaLocations));

    public IEnumerable<MediaLocationImportInfo>? MediaLocations
    {
        get => GetValue(MediaLocationsProperty);
        set => SetValue(MediaLocationsProperty, value);
    }

    // ============================================================
    // Location Count
    // ============================================================

    public static readonly StyledProperty<int> LocationCountProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(LocationCount));

    public int LocationCount
    {
        get => GetValue(LocationCountProperty);
        set => SetValue(LocationCountProperty, value);
    }

    // ============================================================
    // Total Tracks
    // ============================================================

    public static readonly StyledProperty<int> TotalTracksProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalTracks));

    public int TotalTracks
    {
        get => GetValue(TotalTracksProperty);
        set => SetValue(TotalTracksProperty, value);
    }

    // ============================================================
    // Total Folders
    // ============================================================

    public static readonly StyledProperty<int> TotalFoldersProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalFolders));

    public int TotalFolders
    {
        get => GetValue(TotalFoldersProperty);
        set => SetValue(TotalFoldersProperty, value);
    }

    // ============================================================
    // Import Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, ICommand?>(
            nameof(ImportCommand));

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }
}