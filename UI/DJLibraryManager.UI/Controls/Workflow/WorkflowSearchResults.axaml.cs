using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DJLibraryManager.UI.Models.Search;
using System;

namespace DJLibraryManager.UI.Controls.Workflow;

/// <summary>
/// Displays results returned by the Search workflow.
///
/// The control supports category-specific presentation while
/// keeping the Search Workspace independent of the individual
/// result layouts.
///
/// User selections are reported back to the Search Workspace;
/// the control itself does not modify the library.
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

    /// <summary>
    /// Indicates whether the current results represent
    /// duplicate candidates.
    ///
    /// This is presentation state only.
    /// </summary>
    public bool IsDuplicateResults =>
        string.Equals(
            ResultType,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase);

    // ============================================================
    // Search Issue
    // ============================================================

    /// <summary>
    /// The Search issue currently being displayed.
    ///
    /// This contains the persisted SelectedResultId used by
    /// the Search workflow to identify the preferred copy.
    /// </summary>
    public static readonly StyledProperty<SearchIssue?> IssueProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, SearchIssue?>(
            nameof(Issue));

    public SearchIssue? Issue
    {
        get => GetValue(IssueProperty);
        set => SetValue(IssueProperty, value);
    }

    // ============================================================
    // Result Selection
    // ============================================================

    /// <summary>
    /// Raised when the user selects a SearchResult as the
    /// preferred result for the current SearchIssue.
    ///
    /// The Search Workspace owns the actual selection and
    /// persistence logic.
    /// </summary>
    public event EventHandler<SearchResult>? ResultSelected;

    /// <summary>
    /// Handles the Keep This Copy button.
    /// </summary>
    private void KeepResult_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not SearchResult result)
            return;

        SelectResult(result);
    }

    /// <summary>
    /// Reports a SearchResult selection to the Search Workspace.
    /// </summary>
    public void SelectResult(SearchResult? result)
    {
        if (result is null)
            return;

        ResultSelected?.Invoke(
            this,
            result);
    }
}