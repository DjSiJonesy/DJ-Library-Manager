using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Workflow;

/// <summary>
/// Displays results returned by the Search workflow.
///
/// The control supports category-specific presentation while
/// keeping the Search Workspace independent of the individual
/// result layouts.
/// </summary>
public partial class WorkflowSearchResults : UserControl
{
    public WorkflowSearchResults()
    {
        InitializeComponent();
    }

    // ============================================================
    // Results
    // ============================================================

    public static readonly StyledProperty<object?> ResultsProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, object?>(
            nameof(Results));

    public object? Results
    {
        get => GetValue(ResultsProperty);
        set => SetValue(ResultsProperty, value);
    }

    // ============================================================
    // Heading
    // ============================================================

    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, string>(
            nameof(Heading),
            "Search Results");

    public string Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    // ============================================================
    // Result Type
    // ============================================================

    public static readonly StyledProperty<string> ResultTypeProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, string>(
            nameof(ResultType),
            "Duplicates");

    public string ResultType
    {
        get => GetValue(ResultTypeProperty);
        set => SetValue(ResultTypeProperty, value);
    }
}