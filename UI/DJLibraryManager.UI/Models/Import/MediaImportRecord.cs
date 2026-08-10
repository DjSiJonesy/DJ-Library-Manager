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
    /// Discovery date that this import was based on.
    /// Informational only - not used for change detection.
    /// </summary>
    public DateTime? DiscoveryDate { get; set; }

    /// <summary>
    /// Number of folders discovered when this location was imported.
    /// </summary>
    public int FolderCount { get; set; }

    /// <summary>
    /// Number of audio files discovered when this location was imported.
    /// </summary>
    public int AudioFileCount { get; set; }

    /// <summary>
    /// Number of video files discovered when this location was imported.
    /// </summary>
    public int VideoFileCount { get; set; }

    /// <summary>
    /// Total media files discovered at the time of import.
    /// </summary>
    public int TotalFiles =>
        AudioFileCount + VideoFileCount;

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