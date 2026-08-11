using Avalonia;
using Avalonia.Controls;
using DJLibraryManager.UI.Models.Analysis;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Controls.Workflow;

/// <summary>
/// Displays the Analysis issue breakdown table.
/// </summary>
public partial class WorkflowAnalysisTable : UserControl
{
    public static readonly StyledProperty<IEnumerable<AnalysisCategoryInfo>?> CategoriesProperty =
        AvaloniaProperty.Register<WorkflowAnalysisTable, IEnumerable<AnalysisCategoryInfo>?>(
            nameof(Categories));

    public IEnumerable<AnalysisCategoryInfo>? Categories
    {
        get => GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    public WorkflowAnalysisTable()
    {
        InitializeComponent();
    }
}