using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents metadata evidence supplied by an external
/// metadata provider for a candidate track.
///
/// MetadataEvidence is discovery evidence only. It does not
/// modify the DIASISS library and does not represent the final
/// recommendation made by DIASISS.
/// </summary>
public sealed class MetadataEvidence
{
    // ============================================================
    // Source
    // ============================================================

    /// <summary>
    /// Name of the provider that supplied this evidence.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Provider-specific identifier for the candidate recording
    /// or release, when available.
    /// </summary>
    public string ExternalId { get; init; } = string.Empty;

    // ============================================================
    // Metadata
    // ============================================================

    public string Artist { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Album { get; init; } = string.Empty;

    public string Genre { get; init; } = string.Empty;

    /// <summary>
    /// Track year when the provider identifies a year that
    /// specifically belongs to the track or recording.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Year of the release containing the candidate track.
    /// </summary>
    public int? ReleaseYear { get; init; }

    public double? BPM { get; init; }

    public string Key { get; init; } = string.Empty;

    public TimeSpan? Duration { get; init; }

    // ============================================================
    // Provider Evidence
    // ============================================================

    /// <summary>
    /// Confidence assigned by the provider to this candidate.
    ///
    /// This is provider evidence only. It is not the final
    /// DIASISS confidence.
    /// </summary>
    public double ProviderConfidence { get; init; }

    /// <summary>
    /// Explanation supplied by the provider describing why
    /// this candidate was returned.
    /// </summary>
    public string MatchReason { get; init; } = string.Empty;
}