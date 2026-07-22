using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DJLibraryManager.UI.Providers.VirtualDJ.Models;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Services;

/// <summary>
/// Reads VirtualDJ song records from a loaded VirtualDJ database.
/// </summary>
public sealed class VirtualDJSongReader
{
    /// <summary>
    /// Reads all songs from the supplied VirtualDJ database.
    /// </summary>
    public IEnumerable<VirtualDJSong> ReadSongs(VirtualDJDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        foreach (var song in database.Songs)
        {
            var tags = song.Element("Tags");
            var infos = song.Element("Infos");
            var scan = song.Element("Scan");

            yield return new VirtualDJSong
            {
                FilePath = GetAttribute(song, "FilePath"),

                FileSize = ParseLong(
                    GetAttribute(song, "FileSize")),

                Author = GetAttribute(tags, "Author"),

                Title = GetAttribute(tags, "Title"),

                Album = GetAttribute(tags, "Album"),

                Genre = GetAttribute(tags, "Genre"),

                Year = ParseInt(
                    GetAttribute(tags, "Year")),

                BPM = ParseDouble(
                    GetAttribute(scan, "Bpm")),

                Key = GetAttribute(scan, "Key"),

                Duration = TimeSpan.FromSeconds(
                    ParseDouble(GetAttribute(infos, "SongLength")) ?? 0),

                FirstSeen = ParseUnixTime(
                    GetAttribute(infos, "FirstSeen")),

                LastModified = ParseUnixTime(
                    GetAttribute(infos, "LastModified"))
            };
        }
    }

    private static string? GetAttribute(XElement? element, string name)
    {
        return element?.Attribute(name)?.Value;
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, out var result)
            ? result
            : null;
    }

    private static long ParseLong(string? value)
    {
        return long.TryParse(value, out var result)
            ? result
            : 0;
    }

    private static double? ParseDouble(string? value)
    {
        return double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : null;
    }

    private static DateTime? ParseUnixTime(string? value)
    {
        if (!long.TryParse(value, out var seconds))
            return null;

        return DateTimeOffset
            .FromUnixTimeSeconds(seconds)
            .LocalDateTime;
    }
}