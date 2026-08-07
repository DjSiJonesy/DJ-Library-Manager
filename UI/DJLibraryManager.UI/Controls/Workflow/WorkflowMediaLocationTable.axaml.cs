using System.Collections.Generic;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.Core.Models.Discovery;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaLocationTable : UserControl
{
    public WorkflowMediaLocationTable()
    {
        InitializeComponent();
    }

    // ============================================================
    // Media Locations
    // ============================================================

    public static readonly StyledProperty<IEnumerable<MediaLocationDiscoverySummary>?> MediaLocationsProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, IEnumerable<MediaLocationDiscoverySummary>?>(
            nameof(MediaLocations));

    public IEnumerable<MediaLocationDiscoverySummary>? MediaLocations
    {
        get => GetValue(MediaLocationsProperty);
        set => SetValue(MediaLocationsProperty, value);
    }

    // ============================================================
    // Totals
    // ============================================================

    public static readonly StyledProperty<int> TotalFoldersProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, int>(
            nameof(TotalFolders));

    public int TotalFolders
    {
        get => GetValue(TotalFoldersProperty);
        set => SetValue(TotalFoldersProperty, value);
    }

    public static readonly StyledProperty<int> TotalAudioFilesProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, int>(
            nameof(TotalAudioFiles));

    public int TotalAudioFiles
    {
        get => GetValue(TotalAudioFilesProperty);
        set => SetValue(TotalAudioFilesProperty, value);
    }

    public static readonly StyledProperty<int> TotalVideoFilesProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, int>(
            nameof(TotalVideoFiles));

    public int TotalVideoFiles
    {
        get => GetValue(TotalVideoFilesProperty);
        set => SetValue(TotalVideoFilesProperty, value);
    }

    public static readonly StyledProperty<int> TotalDrivesProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, int>(
            nameof(TotalDrives));

    public int TotalDrives
    {
        get => GetValue(TotalDrivesProperty);
        set => SetValue(TotalDrivesProperty, value);
    }

    // ============================================================
    // Discover Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> DiscoverCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, ICommand?>(
            nameof(DiscoverCommand));

    public ICommand? DiscoverCommand
    {
        get => GetValue(DiscoverCommandProperty);
        set => SetValue(DiscoverCommandProperty, value);
    }

    // ============================================================
    // View Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ViewCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationTable, ICommand?>(
            nameof(ViewCommand));

    public ICommand? ViewCommand
    {
        get => GetValue(ViewCommandProperty);
        set => SetValue(ViewCommandProperty, value);
    }
}