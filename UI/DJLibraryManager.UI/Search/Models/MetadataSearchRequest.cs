using DJLibraryManager.UI.Analysis.Models;

using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Describes the information available when searching for
/// missing metadata for a library track.
///
/// Search uses this information to query external metadata
/// providers. It does not modify the DIASISS library.
/// </summary>
public sealed class MetadataSearchRequest
{
    // ============================================================
    // Existing Track Information
    // ============================================================

    /// <summary>
    /// Artist currently stored in the DIASISS library.
    /// </summary>
    public string Artist { get; init; } = string.Empty;

    /// <summary>
    /// Title currently stored in the DIASISS library.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Album currently stored in the DIASISS library.
    /// </summary>
    public string Album { get; init; } = string.Empty;

    /// <summary>
    /// Physical file path of the track.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Duration of the existing track, when available.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    // ============================================================
    // Missing Metadata
    // ============================================================

    /// <summary>
    /// Metadata fields that Analysis has identified as missing.
    ///
    /// Examples:
    /// Artist, Title, Album, Genre, Year, BPM, Key, Duration.
    /// </summary>
    public IReadOnlyList<string> MissingFields { get; init; }
        = Array.Empty<string>();

    // ============================================================
    // Filename Search Hint
    // ============================================================

    /// <summary>
    /// Search information derived from the physical filename.
    ///
    /// These values are hypotheses only and are never treated as
    /// confirmed metadata.
    ///
    /// When Artist and/or Title are missing, Analysis may provide
    /// multiple possible Artist/Title interpretations here.
    ///
    /// External metadata providers can use these candidates to
    /// perform additional searches when normal Artist/Title
    /// metadata is unavailable.
    /// </summary>
    public FilenameSearchHint? FilenameSearchHint { get; init; }
}