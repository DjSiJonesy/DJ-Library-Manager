using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents metadata discovered by an external metadata provider.
///
/// This is discovery data only. It does not modify the DIASISS
/// library.
/// </summary>
public sealed class MetadataSearchProviderResult
{
    // ============================================================
    // Source
    // ============================================================

    /// <summary>
    /// Name of the external provider that supplied the result.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Provider-specific identifier for the recording or release,
    /// when available.
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
    /// Track year when the provider can identify a year that
    /// specifically belongs to the track or recording.
    ///
    /// This should not be populated simply because a release
    /// containing the track has a known release year.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Year of the release containing the discovered track.
    ///
    /// This is deliberately separate from Year because a track
    /// may appear on a later compilation or reissue.
    /// </summary>
    public int? ReleaseYear { get; init; }

    public double? BPM { get; init; }

    public string Key { get; init; } = string.Empty;

    public TimeSpan? Duration { get; init; }

    // ============================================================
    // Confidence
    // ============================================================

    /// <summary>
    /// Provider confidence in the match, expressed as 0-100.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Human-readable explanation of why this result matched.
    /// </summary>
    public string MatchReason { get; init; } = string.Empty;
}