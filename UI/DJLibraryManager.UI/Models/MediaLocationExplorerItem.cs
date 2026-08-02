using System;
using Avalonia.Media;
using System.Collections.Generic;

namespace DJLibraryManager.Core.Models;

/// <summary>
/// Represents everything DIASISS DJ currently knows about a media location.
/// This model is used by the Library Explorer and will gradually be enriched
/// by Discovery, Import, Analysis, Search and Synchronisation.
/// </summary>
public sealed class MediaLocationExplorerItem
{
    /// <summary>
    /// The media location.
    /// </summary>
    public required MediaLocation MediaLocation { get; init; }

    /// <summary>
    /// The latest discovery session for this location.
    /// Null if the location has not yet been discovered.
    /// </summary>
    public DiscoverySession? DiscoverySession { get; init; }

    /// <summary>
    /// True if this location has been discovered.
    /// </summary>
    public bool IsDiscovered => DiscoverySession is not null;

    /// <summary>
    /// Human-readable discovery status.
    /// </summary>
    public string DiscoveryStatus =>
        IsDiscovered
            ? "Discovery Complete"
            : "Not Discovered";

    /// <summary>
    /// Brush used by the status indicator.
    /// Matches the Media Location Workspace.
    /// </summary>
    public IBrush StatusBrush =>
        DiscoveryStatus switch
        {
            "Discovery Complete" => Brushes.LimeGreen,
            "Discovering..." => Brushes.DeepSkyBlue,
            "Location Not Available" => Brushes.DarkOrange,
            _ => Brushes.Gray
        };

    /// <summary>
    /// Text displayed for the last discovery.
    /// </summary>
    public string DiscoveryDateText =>
        DiscoveryDate is null
            ? "Never Discovered"
            : $"Discovered {DiscoveryDate:dd MMM yyyy HH:mm}";

    /// <summary>
    /// Libraries discovered beneath this media location.
    /// Returns an empty collection until discovery has been performed.
    /// </summary>
    public IReadOnlyList<MediaLibrary> Libraries =>
        DiscoverySession?.Libraries ??
        Array.Empty<MediaLibrary>();

    /// <summary>
    /// Number of discovered folders.
    /// </summary>
    public int FolderCount =>
        DiscoverySession?.FolderCount ?? 0;

    /// <summary>
    /// Number of discovered audio files.
    /// </summary>
    public int AudioFileCount =>
        DiscoverySession?.AudioFileCount ?? 0;

    /// <summary>
    /// Number of discovered video files.
    /// </summary>
    public int VideoFileCount =>
        DiscoverySession?.VideoFileCount ?? 0;

    /// <summary>
    /// Total discovered media files.
    /// </summary>
    public int TotalMediaFiles =>
        DiscoverySession?.TotalMediaFiles ?? 0;

    /// <summary>
    /// Total storage used.
    /// </summary>
    public string TotalSize =>
        DiscoverySession?.TotalSize ?? "-";

    /// <summary>
    /// Date the location was last discovered.
    /// </summary>
    public DateTime? DiscoveryDate =>
        DiscoverySession?.DiscoveryDate;
}