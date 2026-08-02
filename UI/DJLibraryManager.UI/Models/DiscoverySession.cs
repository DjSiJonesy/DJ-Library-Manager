using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.Core.Models;

/// <summary>
/// Represents the result of a single media discovery operation.
/// This is the shared model used throughout the application.
/// </summary>
public sealed class DiscoverySession
{
    /// <summary>
    /// The media location that was discovered.
    /// </summary>
    public required MediaLocation MediaLocation { get; init; }

    /// <summary>
    /// The media libraries discovered beneath the selected location.
    /// </summary>
    public required IReadOnlyList<MediaLibrary> Libraries { get; init; }

    /// <summary>
    /// When the discovery completed.
    /// </summary>
    public DateTime DiscoveryDate { get; init; } = DateTime.Now;

    /// <summary>
    /// Total number of discovered folders.
    /// </summary>
    public int FolderCount => Libraries.Count;

    /// <summary>
    /// Total discovered audio files.
    /// </summary>
    public int AudioFileCount =>
        Libraries.Sum(x => x.AudioFileCount);

    /// <summary>
    /// Total discovered video files.
    /// </summary>
    public int VideoFileCount =>
        Libraries.Sum(x => x.VideoFileCount);

    /// <summary>
    /// Total discovered media files.
    /// </summary>
    public int TotalMediaFiles =>
        AudioFileCount + VideoFileCount;

    /// <summary>
    /// Total storage used.
    /// </summary>
    public long TotalSizeBytes =>
        Libraries.Sum(x => x.TotalSizeBytes);

    /// <summary>
    /// Human-readable storage size.
    /// </summary>
    public string TotalSize
    {
        get
        {
            const double kb = 1024;
            const double mb = kb * 1024;
            const double gb = mb * 1024;
            const double tb = gb * 1024;

            if (TotalSizeBytes >= tb)
                return $"{TotalSizeBytes / tb:N2} TB";

            if (TotalSizeBytes >= gb)
                return $"{TotalSizeBytes / gb:N2} GB";

            if (TotalSizeBytes >= mb)
                return $"{TotalSizeBytes / mb:N2} MB";

            if (TotalSizeBytes >= kb)
                return $"{TotalSizeBytes / kb:N2} KB";

            return $"{TotalSizeBytes:N0} Bytes";
        }
    }
}