using System;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Providers.VirtualDJ.Models;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Translators;

/// <summary>
/// Translates a VirtualDJ song into a provider-independent media item.
/// </summary>
public sealed class VirtualDJMediaTranslator
{
    /// <summary>
    /// Converts a VirtualDJ song into a DJLMMediaItem.
    /// </summary>
    public DJLMMediaItem Translate(VirtualDJSong song)
    {
        ArgumentNullException.ThrowIfNull(song);

        return new DJLMMediaItem
        {
            Provider = "VirtualDJ",

            MediaType = "Unknown",

            FilePath = song.FilePath ?? string.Empty,

            FileSize = song.FileSize,

            Artist = song.Author ?? string.Empty,

            Title = song.Title ?? string.Empty,

            Album = song.Album ?? string.Empty,

            Genre = song.Genre ?? string.Empty,

            Year = song.Year,

            BPM = song.BPM,

            Key = song.Key ?? string.Empty,

            Duration = song.Duration,

            DateFirstSeen = song.FirstSeen,

            DateLastModified = song.LastModified
        };
    }
}