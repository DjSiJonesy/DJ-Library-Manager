using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents the persisted Search state for the latest Analysis.
/// </summary>
public sealed class SearchState
{
    /// <summary>
    /// Date and time of the Analysis that produced these Search issues.
    /// </summary>
    public DateTime AnalysisDate { get; init; }

    /// <summary>
    /// When this Search state was saved.
    /// </summary>
    public DateTime SavedAt { get; init; }

    /// <summary>
    /// Search issues and their associated Search results.
    /// </summary>
    public List<SearchIssue> Issues { get; init; } = [];
}