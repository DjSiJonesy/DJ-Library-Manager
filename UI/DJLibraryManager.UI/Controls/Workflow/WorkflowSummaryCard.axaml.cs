using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowSummaryCard : UserControl
{
    public WorkflowSummaryCard()
    {
        InitializeComponent();
    }

    // ============================================================
    // Title
    // ============================================================

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Title),
            string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ============================================================
    // Row 1
    // ============================================================

    public static readonly StyledProperty<string> Label1Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Label1),
            string.Empty);

    public string Label1
    {
        get => GetValue(Label1Property);
        set => SetValue(Label1Property, value);
    }

    public static readonly StyledProperty<string> Value1Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Value1),
            string.Empty);

    public string Value1
    {
        get => GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }

    // ============================================================
    // Row 2
    // ============================================================

    public static readonly StyledProperty<string> Label2Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Label2),
            string.Empty);

    public string Label2
    {
        get => GetValue(Label2Property);
        set => SetValue(Label2Property, value);
    }

    public static readonly StyledProperty<string> Value2Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Value2),
            string.Empty);

    public string Value2
    {
        get => GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }

    // ============================================================
    // Row 3
    // ============================================================

    public static readonly StyledProperty<string> Label3Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Label3),
            string.Empty);

    public string Label3
    {
        get => GetValue(Label3Property);
        set => SetValue(Label3Property, value);
    }

    public static readonly StyledProperty<string> Value3Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Value3),
            string.Empty);

    public string Value3
    {
        get => GetValue(Value3Property);
        set => SetValue(Value3Property, value);
    }

    // ============================================================
    // Row 4
    // ============================================================

    public static readonly StyledProperty<string> Label4Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Label4),
            string.Empty);

    public string Label4
    {
        get => GetValue(Label4Property);
        set => SetValue(Label4Property, value);
    }

    public static readonly StyledProperty<string> Value4Property =
        AvaloniaProperty.Register<WorkflowSummaryCard, string>(
            nameof(Value4),
            string.Empty);

    public string Value4
    {
        get => GetValue(Value4Property);
        set => SetValue(Value4Property, value);
    }
}