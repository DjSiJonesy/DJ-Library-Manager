using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Analysis.Models;

/// <summary>
/// Represents the complete result of analysing a media library.
/// </summary>
public sealed class LibraryAnalysisResult
{
    /// <summary>
    /// Date and time the analysis completed.
    /// </summary>
    public DateTime AnalysisDate { get; init; } = DateTime.Now;

    /// <summary>
    /// Total number of tracks analysed.
    /// </summary>
    public int TracksScanned { get; init; }

    /// <summary>
    /// Overall library health score (0-100).
    /// </summary>
    public double HealthScore { get; init; }

    /// <summary>
    /// Analysis results grouped by category.
    /// </summary>
    public IReadOnlyList<AnalysisCategoryResult> Categories { get; init; } = [];

    /// <summary>
    /// Total number of issues discovered.
    /// </summary>
    public int TotalIssues => Categories.Sum(c => c.IssueCount);
}