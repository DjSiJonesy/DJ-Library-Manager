using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents the persisted Search state for the latest Analysis.
///
/// SearchState contains both the individual Search results and
/// information about any Search Run that produced them.
/// </summary>
public sealed class SearchState
{
    // ============================================================
    // Analysis
    // ============================================================

    /// <summary>
    /// Date and time of the Analysis that produced these Search issues.
    /// </summary>
    public DateTime AnalysisDate { get; init; }

    // ============================================================
    // Persistence
    // ============================================================

    /// <summary>
    /// When this Search state was last saved.
    /// </summary>
    public DateTime SavedAt { get; init; }

    // ============================================================
    // Search Run
    // ============================================================

    /// <summary>
    /// Current or most recent Search Run.
    ///
    /// Null when no Search All operation has been started.
    /// </summary>
    public SearchRun? Run { get; set; }

    // ============================================================
    // Issues
    // ============================================================

    /// <summary>
    /// Search issues and their associated Search results.
    /// </summary>
    public List<SearchIssue> Issues { get; init; } = [];
}