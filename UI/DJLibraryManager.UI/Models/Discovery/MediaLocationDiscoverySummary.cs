using System;
using Avalonia.Media;
using DJLibraryManager.Core.Models;

namespace DJLibraryManager.Core.Models.Discovery;

/// <summary>
/// Represents the current discovery summary for a media location.
/// This is generated from a DiscoverySession and is used by multiple
/// workspaces (Discovery, Media Location, Import, etc.).
/// </summary>
public sealed class MediaLocationDiscoverySummary
{
    /// <summary>
    /// The media location this summary relates to.
    /// </summary>
    public required MediaLocation MediaLocation { get; init; }

    /// <summary>
    /// Date and time the discovery was performed.
    /// Null if discovery has not yet been run.
    /// </summary>
    public DateTime? DiscoveryDate { get; init; }

    /// <summary>
    /// Number of media folders discovered.
    /// </summary>
    public int FolderCount { get; init; }

    /// <summary>
    /// Number of audio files discovered.
    /// </summary>
    public int AudioFileCount { get; init; }

    /// <summary>
    /// Number of video files discovered.
    /// </summary>
    public int VideoFileCount { get; init; }

    /// <summary>
    /// Combined audio and video file count.
    /// </summary>
    public int TotalMediaFiles => AudioFileCount + VideoFileCount;

    /// <summary>
    /// Total size of all discovered media.
    /// </summary>
    public long TotalSizeBytes { get; init; }

    /// <summary>
    /// Friendly discovery status.
    /// </summary>
    public string Status { get; init; } = "Ready to Discover";

    /// <summary>
    /// Colour used when displaying the discovery status.
    /// </summary>
    public IBrush StatusBrush =>
        Status switch
        {
            "Discovery Complete" => Brushes.LimeGreen,
            "Discovering..." => Brushes.DeepSkyBlue,
            "Location Not Available" => Brushes.DarkOrange,
            _ => Brushes.Gray
        };

    /// <summary>
    /// True once discovery has completed successfully.
    /// </summary>
    public bool CanView =>
        DiscoveryDate is not null;

    /// <summary>
    /// True when discovery can be started from the Discovery workspace.
    /// </summary>
    public bool CanDiscover =>
        DiscoveryDate is null &&
        MediaLocation.Exists;

}