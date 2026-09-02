using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// The authoritative DIASISS MediaId associated with this result.
    ///
    /// This is the GUID created by the Import/Library layer and is
    /// carried through Search into Improve and Structure.
    ///
    /// SearchResult.Id remains the identity of the Search result itself
    /// and must not be used as the DIASISS media identity.
    /// </summary>
    [ObservableProperty]
    private string mediaId = string.Empty;


    // ============================================================
    // Source
    // ============================================================

    [ObservableProperty]
    private string source = string.Empty;

    // ============================================================
    // Match / Recommendation
    // ============================================================

    /// <summary>
    /// Confidence or suitability score for this result.
    ///
    /// This is currently used by the Search recommendation
    /// system. It will eventually represent the final Keep
    /// Recommendation score.
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
    // Selection
    // ============================================================

    /// <summary>
    /// Indicates whether this result is currently selected by
    /// the user as the preferred copy to keep.
    ///
    /// This is presentation state only and is deliberately not
    /// persisted. The persisted source of truth is
    /// SearchIssue.SelectedResultId.
    /// </summary>
    [ObservableProperty]
    [JsonIgnore]
    private bool isSelected;

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

    // ============================================================
    // Physical File Inspection
    // ============================================================

    /// <summary>
    /// Indicates whether the physical file has been inspected.
    ///
    /// Null means inspection has not yet been performed.
    /// </summary>
    [ObservableProperty]
    private bool? isInspected;

    /// <summary>
    /// Indicates whether the physical file passed the current
    /// integrity/readability checks.
    ///
    /// Null means the file has not yet been inspected.
    /// </summary>
    [ObservableProperty]
    private bool? isHealthy;

    /// <summary>
    /// Human-readable description of the physical file integrity
    /// result.
    /// </summary>
    [ObservableProperty]
    private string integrityStatus = string.Empty;

    // ============================================================
    // Audio Format
    // ============================================================

    /// <summary>
    /// Physical file format, for example MP3, FLAC or WAV.
    /// </summary>
    [ObservableProperty]
    private string format = string.Empty;

    /// <summary>
    /// Audio codec reported by the physical file.
    /// </summary>
    [ObservableProperty]
    private string codec = string.Empty;

    /// <summary>
    /// Indicates whether the physical audio format is lossless.
    ///
    /// Null means the format could not be classified.
    /// </summary>
    [ObservableProperty]
    private bool? isLossless;

    // ============================================================
    // Audio Quality
    // ============================================================

    /// <summary>
    /// Audio bitrate in bits per second, when available.
    /// </summary>
    [ObservableProperty]
    private int? bitrate;

    /// <summary>
    /// Audio sample rate in Hz, when available.
    /// </summary>
    [ObservableProperty]
    private int? sampleRate;

    /// <summary>
    /// Audio bit depth, when available.
    /// </summary>
    [ObservableProperty]
    private int? bitDepth;

    /// <summary>
    /// Number of audio channels, when available.
    /// </summary>
    [ObservableProperty]
    private int? channels;
}