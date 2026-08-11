namespace DJLibraryManager.UI.Analysis.Models;

/// <summary>
/// Versioned wrapper for persisted analysis.
/// </summary>
public sealed class AnalysisDocument
{
    /// <summary>
    /// File format version.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// The latest analysis.
    /// </summary>
    public LibraryAnalysisResult Analysis { get; init; } = new();
}