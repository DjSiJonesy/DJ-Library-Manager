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

    /// <summary>
    /// The file this issue relates to.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Other files belonging to the same issue.
    ///
    /// Used by duplicate analysis to represent the complete
    /// duplicate group.
    /// </summary>
    public IReadOnlyList<string> RelatedFilePaths { get; init; }
        = [];

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