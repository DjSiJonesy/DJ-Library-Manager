using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Models;

/// <summary>
/// Represents a single issue discovered during library analysis.
/// </summary>
public sealed class AnalysisIssue
{
    /// <summary>
    /// Unique identifier for this issue.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Category this issue belongs to.
    /// Example: Metadata, Files, Duplicates.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Type of issue.
    /// Example: MetadataIncomplete, DuplicateTrack.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Detailed description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    // ============================================================
    // Media
    // ============================================================

    /// <summary>
    /// Artist associated with the affected track.
    ///
    /// This represents the Artist currently stored in the
    /// DIASISS library. It is not a filename-derived value.
    /// </summary>
    public string Artist { get; init; } = string.Empty;

    /// <summary>
    /// Title of the affected track.
    ///
    /// This represents the Title currently stored in the
    /// DIASISS library. It is not a filename-derived value.
    /// </summary>
    public string TrackTitle { get; init; } = string.Empty;

    /// <summary>
    /// Album associated with the affected track.
    ///
    /// This represents the Album currently stored in the
    /// DIASISS library. It is not a filename-derived value.
    /// </summary>
    public string Album { get; init; } = string.Empty;

    /// <summary>
    /// Duration of the affected track, when available.
    ///
    /// This represents the Duration currently stored in the
    /// DIASISS library and may be used as supporting evidence
    /// when matching external metadata.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// The file this issue relates to.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    // ============================================================
    // Filename Search Hints
    // ============================================================

    /// <summary>
    /// Search information derived from the physical filename.
    ///
    /// These are search hints only. They are not confirmed
    /// metadata and must not be written back to the DIASISS
    /// library as Artist or Title values.
    ///
    /// This is normally populated when Analysis identifies that
    /// Artist and/or Title metadata is missing.
    /// </summary>
    public FilenameSearchHint? FilenameSearchHint { get; init; }

    // ============================================================
    // Related Files
    // ============================================================

    /// <summary>
    /// Other files belonging to the same issue.
    ///
    /// Used by duplicate analysis to represent the complete
    /// duplicate group.
    /// </summary>
    public IReadOnlyList<string> RelatedFilePaths { get; init; }
        = [];

    // ============================================================
    // Metadata
    // ============================================================

    /// <summary>
    /// Metadata fields that are missing from the affected track.
    ///
    /// Used by Metadata analysis. Empty for issue types where
    /// missing metadata fields are not relevant.
    /// </summary>
    public IReadOnlyList<string> MissingFields { get; init; }
        = [];

    /// <summary>
    /// Indicates whether this issue can be automatically corrected.
    /// </summary>
    public bool CanAutoFix { get; init; }
}