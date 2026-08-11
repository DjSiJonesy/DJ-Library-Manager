using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowAnalysisSummaryCard : UserControl
{
    public WorkflowAnalysisSummaryCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<int> TracksScannedProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, int>(
            nameof(TracksScanned));

    public int TracksScanned
    {
        get => GetValue(TracksScannedProperty);
        set => SetValue(TracksScannedProperty, value);
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

    public static readonly StyledProperty<string> LastAnalysedProperty =
        AvaloniaProperty.Register<WorkflowAnalysisSummaryCard, string>(
            nameof(LastAnalysed),
            "Never");

    public string LastAnalysed
    {
        get => GetValue(LastAnalysedProperty);
        set => SetValue(LastAnalysedProperty, value);
    }
}