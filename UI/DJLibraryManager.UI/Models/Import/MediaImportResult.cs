namespace DJLibraryManager.UI.Models.Import;

/// <summary>
/// Result returned after importing one or more media locations.
/// </summary>
public sealed class MediaImportResult
{
    /// <summary>
    /// Total files scanned.
    /// </summary>
    public int Scanned { get; set; }

    /// <summary>
    /// Number of files imported into the DIASISS library.
    /// </summary>
    public int Imported { get; set; }

    /// <summary>
    /// Number of files skipped because they already exist.
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Number of files that failed to import.
    /// </summary>
    public int Failed { get; set; }
}