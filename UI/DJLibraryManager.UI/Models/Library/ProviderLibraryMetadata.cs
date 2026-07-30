using System;

namespace DJLibraryManager.UI.Models.Library;

/// <summary>
/// Represents the persisted metadata for a provider library.
/// </summary>
public sealed class ProviderLibraryMetadata
{
    /// <summary>
    /// Name of the provider (VirtualDJ, Rekordbox, Serato, etc.).
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time the provider library was last imported.
    /// </summary>
    public DateTime LastImported { get; set; }

    /// <summary>
    /// Number of tracks imported.
    /// </summary>
    public int TrackCount { get; set; }

    /// <summary>
    /// Number of playlists imported.
    /// </summary>
    public int PlaylistCount { get; set; }
}