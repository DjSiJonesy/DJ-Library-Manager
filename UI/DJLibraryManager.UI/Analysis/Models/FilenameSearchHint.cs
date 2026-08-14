using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Models;

/// <summary>
/// Represents search information derived from a track filename.
///
/// These values are search hints only. They are not treated as
/// confirmed Artist or Title metadata and must not modify the
/// DIASISS library.
///
/// Analysis may produce multiple possible interpretations because
/// filenames can use different conventions, such as:
///
///     Artist - Title
///     Title - Artist
///
/// Search is responsible for using these hints when querying
/// external metadata providers.
/// </summary>
public sealed class FilenameSearchHint
{
    // ============================================================
    // Original Filename
    // ============================================================

    /// <summary>
    /// The original filename, including its extension.
    /// </summary>
    public string Filename { get; init; } =
        string.Empty;

    // ============================================================
    // Cleaned Filename
    // ============================================================

    /// <summary>
    /// Filename with the file extension and recognised technical
    /// suffixes removed.
    ///
    /// Example:
    ///
    /// 50 Cent - Candy Shop (720p60fps).mp4
    ///
    /// becomes:
    ///
    /// 50 Cent - Candy Shop
    /// </summary>
    public string CleanedFilename { get; init; } =
        string.Empty;

    // ============================================================
    // Filename Parts
    // ============================================================

    /// <summary>
    /// First meaningful part of the filename.
    ///
    /// This is deliberately not called Artist because the filename
    /// convention may be either Artist - Title or Title - Artist.
    /// </summary>
    public string PartA { get; init; } =
        string.Empty;

    /// <summary>
    /// Second meaningful part of the filename.
    ///
    /// This is deliberately not called Title because the filename
    /// convention may be either Artist - Title or Title - Artist.
    /// </summary>
    public string PartB { get; init; } =
        string.Empty;

    // ============================================================
    // Search Candidates
    // ============================================================

    /// <summary>
    /// Possible interpretations of the filename that Search can
    /// investigate using external metadata providers.
    /// </summary>
    public IReadOnlyList<FilenameSearchCandidate> Candidates { get; init; }
        = [];
}

/// <summary>
/// Represents one possible interpretation of a filename.
///
/// The values are hypotheses for external searching only.
/// They are not confirmed metadata.
/// </summary>
public sealed class FilenameSearchCandidate
{
    /// <summary>
    /// Possible Artist value.
    /// </summary>
    public string Artist { get; init; } =
        string.Empty;

    /// <summary>
    /// Possible Title value.
    /// </summary>
    public string Title { get; init; } =
        string.Empty;

    /// <summary>
    /// Human-readable description of how this candidate was
    /// derived.
    ///
    /// Examples:
    ///
    /// "Filename interpreted as Artist - Title"
    /// "Filename interpreted as Title - Artist"
    /// </summary>
    public string Interpretation { get; init; } =
        string.Empty;
}