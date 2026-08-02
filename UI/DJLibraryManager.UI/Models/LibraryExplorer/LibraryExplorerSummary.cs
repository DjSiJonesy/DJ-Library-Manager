namespace DJLibraryManager.UI.Models.LibraryExplorer;

/// <summary>
/// Summary information displayed by the Library Explorer.
/// </summary>
public sealed class LibraryExplorerSummary
{
    /// <summary>
    /// Number of discovered media locations.
    /// </summary>
    public int MediaLocationCount { get; init; }

    /// <summary>
    /// Number of discovered libraries (folders).
    /// </summary>
    public int LibraryCount { get; init; }

    /// <summary>
    /// Total discovered audio files.
    /// </summary>
    public int AudioFileCount { get; init; }

    /// <summary>
    /// Total discovered video files.
    /// </summary>
    public int VideoFileCount { get; init; }

    /// <summary>
    /// Combined media file count.
    /// </summary>
    public int TotalMediaFiles =>
        AudioFileCount + VideoFileCount;

    /// <summary>
    /// Total discovered storage.
    /// </summary>
    public long TotalSizeBytes { get; init; }

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