using System;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents the state of a Search All operation.
///
/// A SearchRun records the progress of the operation separately
/// from the individual SearchIssue results so an interrupted
/// Search can be resumed later.
/// </summary>
public sealed class SearchRun
{
    // ============================================================
    // Identity
    // ============================================================

    /// <summary>
    /// Date and time of the Analysis that this Search Run belongs to.
    /// </summary>
    public DateTime AnalysisDate { get; init; }

    /// <summary>
    /// Search category being processed.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    // ============================================================
    // Status
    // ============================================================

    /// <summary>
    /// Current state of the Search Run.
    ///
    /// Expected values:
    /// NotStarted
    /// Running
    /// Completed
    /// Failed
    /// </summary>
    public string Status { get; set; } = "NotStarted";

    // ============================================================
    // Timing
    // ============================================================

    /// <summary>
    /// When the Search Run started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the Search Run completed.
    ///
    /// Null while the Search Run is still active or was interrupted.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // ============================================================
    // Progress
    // ============================================================

    /// <summary>
    /// Total number of issues in the Search Run.
    /// </summary>
    public int TotalIssues { get; set; }

    /// <summary>
    /// Number of issues that have been successfully searched.
    /// </summary>
    public int IssuesSearched { get; set; }

    /// <summary>
    /// Number of searched issues that produced one or more results.
    /// </summary>
    public int IssuesWithResults { get; set; }
}