using System;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents the issues identified by Analysis that are available
/// for the Search workflow to investigate.
/// </summary>
public sealed class SearchSummary
{
    // ============================================================
    // Duplicates
    // ============================================================

    /// <summary>
    /// Number of duplicate issues identified by Analysis.
    /// </summary>
    public int DuplicateCount { get; set; }

    // ============================================================
    // Missing Files
    // ============================================================

    /// <summary>
    /// Number of missing file issues identified by Analysis.
    /// </summary>
    public int MissingFileCount { get; set; }

    // ============================================================
    // Metadata
    // ============================================================

    /// <summary>
    /// Number of metadata issues identified by Analysis.
    /// </summary>
    public int MetadataIssueCount { get; set; }

    // ============================================================
    // Music
    // ============================================================

    /// <summary>
    /// Number of music-related issues identified by Analysis.
    /// </summary>
    public int MusicIssueCount { get; set; }

    // ============================================================
    // Providers
    // ============================================================

    /// <summary>
    /// Number of provider-related issues identified by Analysis.
    /// </summary>
    public int ProviderIssueCount { get; set; }

    // ============================================================
    // Analysis
    // ============================================================

    /// <summary>
    /// Date and time of the analysis that produced these issues.
    /// </summary>
    public DateTime? AnalysisDate { get; set; }

    /// <summary>
    /// Indicates whether an analysis result is available.
    /// </summary>
    public bool HasAnalysis =>
        AnalysisDate.HasValue;
}