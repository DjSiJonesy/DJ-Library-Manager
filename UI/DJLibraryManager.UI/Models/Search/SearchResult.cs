using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents a possible result found while investigating
/// a SearchIssue.
///
/// Search results are recommendations only. They do not modify
/// the DIASISS library.
/// </summary>
public partial class SearchResult : ObservableObject
{
    // ============================================================
    // Identity
    // ============================================================

    [ObservableProperty]
    private string id = string.Empty;

    // ============================================================
    // Source
    // ============================================================

    [ObservableProperty]
    private string source = string.Empty;

    // ============================================================
    // Match
    // ============================================================

    /// <summary>
    /// Confidence or suitability score for this result.
    /// </summary>
    [ObservableProperty]
    private double matchScore;

    /// <summary>
    /// Indicates whether Search recommends this result
    /// as the preferred candidate.
    /// </summary>
    [ObservableProperty]
    private bool isRecommended;

    /// <summary>
    /// Explains why this candidate has been recommended.
    /// </summary>
    [ObservableProperty]
    private string recommendationReason = string.Empty;

    // ============================================================
    // Media
    // ============================================================

    [ObservableProperty]
    private string artist = string.Empty;

    [ObservableProperty]
    private string trackTitle = string.Empty;

    [ObservableProperty]
    private string album = string.Empty;

    [ObservableProperty]
    private string genre = string.Empty;

    [ObservableProperty]
    private double? bpm;

    [ObservableProperty]
    private string key = string.Empty;

    [ObservableProperty]
    private TimeSpan? duration;

    [ObservableProperty]
    private string filePath = string.Empty;

    // ============================================================
    // File Information
    // ============================================================

    /// <summary>
    /// File size in bytes, when available.
    /// </summary>
    [ObservableProperty]
    private long? fileSize;

    /// <summary>
    /// Indicates whether the candidate file currently exists.
    /// </summary>
    [ObservableProperty]
    private bool fileExists;
}