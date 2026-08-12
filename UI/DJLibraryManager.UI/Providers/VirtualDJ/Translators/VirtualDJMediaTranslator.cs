using System;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Providers.VirtualDJ.Models;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Translators;

/// <summary>
/// Translates a VirtualDJ song into a provider-independent media item.
///
/// VirtualDJ stores BPM as the duration of one beat in seconds.
/// DIASISS stores BPM as beats per minute, so the value is converted
/// during translation at the provider boundary.
/// </summary>
public sealed class VirtualDJMediaTranslator
{
    /// <summary>
    /// Converts a VirtualDJ song into a DJLMMediaItem.
    /// </summary>
    public DJLMMediaItem Translate(
        VirtualDJSong song)
    {
        ArgumentNullException.ThrowIfNull(song);

        return new DJLMMediaItem
        {
            Provider = "VirtualDJ",

            MediaType = "Unknown",

            FilePath =
                song.FilePath ?? string.Empty,

            FileSize =
                song.FileSize,

            Artist =
                song.Author ?? string.Empty,

            Title =
                song.Title ?? string.Empty,

            Album =
                song.Album ?? string.Empty,

            Genre =
                song.Genre ?? string.Empty,

            Year =
                song.Year,

            BPM =
                ConvertVirtualDJBpm(
                    song.BPM),

            Key =
                song.Key ?? string.Empty,

            Duration =
                song.Duration,

            DateFirstSeen =
                song.FirstSeen,

            DateLastModified =
                song.LastModified
        };
    }

    // ============================================================
    // VirtualDJ BPM Conversion
    // ============================================================

    /// <summary>
    /// Converts the VirtualDJ BPM representation into
    /// beats per minute.
    ///
    /// VirtualDJ stores the duration of one beat in seconds.
    ///
    /// Example:
    ///
    /// 0.606077 seconds per beat
    /// 60 / 0.606077
    /// = approximately 99 BPM
    /// </summary>
    private static double? ConvertVirtualDJBpm(
        double? value)
    {
        if (!value.HasValue)
            return null;

        if (value.Value <= 0)
            return null;

        var bpm =
            60.0 / value.Value;

        // Reject invalid or nonsensical values.
        if (double.IsNaN(bpm) ||
            double.IsInfinity(bpm) ||
            bpm <= 0)
        {
            return null;
        }

        return bpm;
    }
}