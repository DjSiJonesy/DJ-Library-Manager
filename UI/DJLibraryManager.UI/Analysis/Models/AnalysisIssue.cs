using System;

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
    /// Example: MissingGenre, DuplicateTrack.
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
    /// Indicates whether this issue can be automatically corrected.
    /// </summary>
    public bool CanAutoFix { get; init; }
}