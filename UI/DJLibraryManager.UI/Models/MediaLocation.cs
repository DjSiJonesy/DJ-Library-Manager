using System;
using Avalonia.Media;

namespace DJLibraryManager.Core.Models;

/// <summary>
/// Represents a discovered media location on the local computer.
///
/// A MediaLocation is simply a location that may contain music or video.
/// It does not contain any analysis information. Analysis is performed
/// later by the library analysis engine when the user chooses to analyse
/// the library.
/// </summary>
public sealed class MediaLocation
{
    /// <summary>
    /// Friendly name displayed in the UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full path to the media location.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Drive that contains this media location.
    /// Example: C:\ or D:\
    /// </summary>
    public string Drive { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the location currently exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// True if this location was discovered automatically.
    /// False if it was added manually by the user.
    /// </summary>
    public bool AutoDiscovered { get; set; } = true;

    /// <summary>
    /// Date and time the location was discovered.
    /// </summary>
    public DateTime DiscoveredOn { get; set; } = DateTime.Now;

    // ============================================================
    // Discovery Statistics
    // ============================================================

    /// <summary>
    /// Number of folders discovered beneath this location.
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

    // ============================================================
    // Discovery Status
    // ============================================================

    /// <summary>
    /// Indicates whether this location has been scanned.
    /// </summary>
    public bool DiscoveryComplete { get; set; }

    /// <summary>
    /// Friendly discovery status.
    /// </summary>
    public string DiscoveryStatus =>
        DiscoveryComplete
            ? "Discovery Complete"
            : "Ready to Discover";

    /// <summary>
    /// Colour used by the workflow status indicator.
    /// </summary>
    public IBrush DiscoveryStatusBrush =>
        DiscoveryComplete
            ? Brushes.LimeGreen
            : Brushes.Orange;

    /// <summary>
    /// Returns the display name.
    /// </summary>
    public override string ToString()
    {
        return Name;
    }
}