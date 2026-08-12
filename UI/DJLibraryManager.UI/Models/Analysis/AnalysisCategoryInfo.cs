using DJLibraryManager.UI.Analysis.Models;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Models.Analysis;

/// <summary>
/// Represents a single row within the Analysis table.
///
/// In addition to the summary information displayed in the
/// Issue Breakdown table, the category retains the issues
/// identified by Analysis so they can be displayed in the
/// Analysis detail area.
/// </summary>
public sealed class AnalysisCategoryInfo
{
    /// <summary>
    /// Name of the analysis category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Explains what the analysis category checks.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of issues identified by this category.
    /// </summary>
    public int IssueCount { get; set; }

    /// <summary>
    /// Health score for this category.
    /// </summary>
    public double HealthScore { get; set; }

    /// <summary>
    /// Issues identified by this analysis category.
    ///
    /// The issues are retained so the Analysis Workspace can
    /// display richer information without running Analysis again.
    /// </summary>
    public IReadOnlyList<AnalysisIssue> Issues { get; set; } = [];
}