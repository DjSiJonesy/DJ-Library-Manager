namespace DJLibraryManager.Core.Models.Library;

/// <summary>
/// Represents the current statistics for the DIASISS Library.
///
/// This model acts as the single source of truth for all
/// library-related statistics displayed throughout the application.
/// </summary>
public sealed class LibraryStatistics
{
    // ============================================================
    // DJLM Library
    // ============================================================

    /// <summary>
    /// Total tracks currently stored in the DIASISS Library.
    /// </summary>
    public int LibraryTrackCount { get; init; }

    /// <summary>
    /// Total playlists imported into the DIASISS Library.
    /// </summary>
    public int LibraryPlaylistCount { get; init; }

    // ============================================================
    // Provider Imports
    // ============================================================

    /// <summary>
    /// Total tracks imported from provider libraries.
    /// </summary>
    public int ProviderTrackCount { get; init; }

    /// <summary>
    /// Total playlists imported from providers.
    /// </summary>
    public int ProviderPlaylistCount { get; init; }

    // ============================================================
    // Media Imports
    // ============================================================

    /// <summary>
    /// Total media files discovered.
    /// </summary>
    public int DiscoveredTrackCount { get; init; }

    /// <summary>
    /// Total new media files imported.
    /// </summary>
    public int ImportedMediaTrackCount { get; init; }

    /// <summary>
    /// Media files that already existed in the DIASISS Library.
    /// </summary>
    public int ExistingMediaTrackCount { get; init; }

    /// <summary>
    /// Media files that failed to import.
    /// </summary>
    public int FailedMediaImports { get; init; }
}