using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DJLibraryManager.UI.Controls.Workflow;

/// <summary>
/// Displays the Analysis workflow summary.
/// </summary>
public partial class WorkflowAnalysisSummaryCard : UserControl
{
    public WorkflowAnalysisSummaryCard()
    {
        InitializeComponent();
    }

    // ============================================================
    // Summary
    // ============================================================

    public static readonly StyledProperty<int> TracksScannedProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, int>(
            nameof(TracksScanned));

    public int TracksScanned
    {
        get => GetValue(TracksScannedProperty);
        set => SetValue(TracksScannedProperty, value);
    }

    public static readonly StyledProperty<int> TotalTracksProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, int>(
            nameof(TotalTracks));

    public int TotalTracks
    {
        get => GetValue(TotalTracksProperty);
        set => SetValue(TotalTracksProperty, value);
    }

    public static readonly StyledProperty<int> IssuesFoundProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, int>(
            nameof(IssuesFound));

    public int IssuesFound
    {
        get => GetValue(IssuesFoundProperty);
        set => SetValue(IssuesFoundProperty, value);
    }

    public static readonly StyledProperty<double> HealthScoreProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, double>(
            nameof(HealthScore));

    public double HealthScore
    {
        get => GetValue(HealthScoreProperty);
        set => SetValue(HealthScoreProperty, value);
    }

    // ============================================================
    // Analysis
    // ============================================================

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, double>(
            nameof(Progress));

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, string>(
            nameof(Status),
            "Ready");

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly StyledProperty<IBrush> StatusBrushProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, IBrush>(
            nameof(StatusBrush),
            Brushes.Gray);

    public IBrush StatusBrush
    {
        get => GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    public static readonly StyledProperty<string> CurrentTrackProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, string>(
            nameof(CurrentTrack),
            "—");

    public string CurrentTrack
    {
        get => GetValue(CurrentTrackProperty);
        set => SetValue(CurrentTrackProperty, value);
    }

    // ============================================================
    // Dates
    // ============================================================

    public static readonly StyledProperty<DateTime?> LastAnalysedProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, DateTime?>(
            nameof(LastAnalysed));

    public DateTime? LastAnalysed
    {
        get => GetValue(LastAnalysedProperty);
        set => SetValue(LastAnalysedProperty, value);
    }

    // ============================================================
    // Commands
    // ============================================================

    public static readonly StyledProperty<ICommand?> AnalyseCommandProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, ICommand?>(
            nameof(AnalyseCommand));

    public ICommand? AnalyseCommand
    {
        get => GetValue(AnalyseCommandProperty);
        set => SetValue(AnalyseCommandProperty, value);
    }
}