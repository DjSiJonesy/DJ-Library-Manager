namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Versioned persistence document for Search state.
/// </summary>
public sealed class SearchDocument
{
    /// <summary>
    /// Persistence document version.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Persisted Search state.
    /// </summary>
    public SearchState? Search { get; init; }
}