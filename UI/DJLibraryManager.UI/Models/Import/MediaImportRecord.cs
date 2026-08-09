using System;

namespace DJLibraryManager.UI.Models.Import;

/// <summary>
/// Persisted import information for a media location.
/// </summary>
public sealed class MediaImportRecord
{
    /// <summary>
    /// Unique identifier for the media location.
    /// Currently the full path.
    /// </summary>
    public string LocationPath { get; set; } = string.Empty;

    /// <summary>
    /// Current import state.
    /// </summary>
    public MediaImportState ImportState { get; set; } =
        MediaImportState.Ready;

    /// <summary>
    /// Date/time the location was last imported.
    /// </summary>
    public DateTime? LastImported { get; set; }

    /// <summary>
    /// Number of files imported during the last successful import.
    /// </summary>
    public int ImportedFiles { get; set; }

    /// <summary>
    /// Number of files skipped during the last successful import.
    /// </summary>
    public int SkippedFiles { get; set; }

    /// <summary>
    /// Number of files that failed during the last successful import.
    /// </summary>
    public int FailedFiles { get; set; }
}