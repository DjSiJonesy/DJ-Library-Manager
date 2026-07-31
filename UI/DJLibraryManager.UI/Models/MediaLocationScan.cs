using System;
using System.Collections.Generic;

namespace DJLibraryManager.Core.Models;

/// <summary>
/// Represents the results of scanning a media location.
///
/// A Media Location Scan is an inventory of everything found beneath
/// a selected media location. It contains summary statistics that are
/// later used by the Analysis, Recovery and Reporting modules.
/// </summary>
public sealed class MediaLocationScan
{
    /// <summary>
    /// The scanned media location.
    /// </summary>
    public MediaLocation MediaLocation { get; set; } = new();

    /// <summary>
    /// Date and time the scan started.
    /// </summary>
    public DateTime Started { get; set; }

    /// <summary>
    /// Date and time the scan completed.
    /// </summary>
    public DateTime Finished { get; set; }

    /// <summary>
    /// Total scan duration.
    /// </summary>
    public TimeSpan Duration => Finished - Started;

    /// <summary>
    /// Total folders discovered.
    /// </summary>
    public int FolderCount { get; set; }

    /// <summary>
    /// Number of audio files discovered.
    /// </summary>
    public int AudioFileCount { get; set; }

    /// <summary>
    /// Number of video files discovered.
    /// </summary>
    public int VideoFileCount { get; set; }

    /// <summary>
    /// Number of artwork files discovered.
    /// </summary>
    public int ArtworkFileCount { get; set; }

    /// <summary>
    /// Number of all other files discovered.
    /// </summary>
    public int OtherFileCount { get; set; }

    /// <summary>
    /// Total files discovered.
    /// </summary>
    public int TotalFileCount =>
        AudioFileCount +
        VideoFileCount +
        ArtworkFileCount +
        OtherFileCount;

    /// <summary>
    /// Total size of all discovered media.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Libraries discovered within this media location.
    /// </summary>
    public List<MediaLibrary> Libraries { get; } = new();

    /// <summary>
    /// Indicates whether the scan completed successfully.
    /// </summary>
    public bool Successful { get; set; }

    /// <summary>
    /// Optional error message if the scan failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}