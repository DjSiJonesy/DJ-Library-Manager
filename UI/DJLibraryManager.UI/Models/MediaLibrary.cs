namespace DJLibraryManager.Core.Models;

public class MediaLibrary
{
    /// <summary>
    /// Display name shown in the UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Root path of the library.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Drive letter (C:, D:, etc.)
    /// </summary>
    public string Drive { get; set; } = string.Empty;

    /// <summary>
    /// Number of supported audio files.
    /// </summary>
    public int AudioFileCount { get; set; }

    /// <summary>
    /// Number of supported video files.
    /// </summary>
    public int VideoFileCount { get; set; }

    /// <summary>
    /// Total number of media files.
    /// </summary>
    public int TotalMediaFiles => AudioFileCount + VideoFileCount;

    /// <summary>
    /// Total size of all media.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Indicates this appears to be a top-level media library.
    /// </summary>
    public bool IsLibraryRoot { get; set; }

    /// <summary>
    /// Where the library was discovered.
    /// </summary>
    public string Source { get; set; } = "Discovery";
}