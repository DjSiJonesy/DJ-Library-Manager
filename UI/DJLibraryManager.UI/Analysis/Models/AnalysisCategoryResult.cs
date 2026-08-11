using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Models;

/// <summary>
/// Represents the outcome of analysing a single category.
/// </summary>
public sealed class AnalysisCategoryResult
{
    /// <summary>
    /// Name of the category.
    /// Example: Metadata, Duplicates, File Integrity.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Total number of issues found.
    /// </summary>
    public int IssueCount => Issues.Count;

    /// <summary>
    /// Category health score.
    /// 0–100.
    /// </summary>
    public double HealthScore { get; init; }

    /// <summary>
    /// Collection of issues discovered.
    /// </summary>
    public IReadOnlyList<AnalysisIssue> Issues { get; init; } = [];
}