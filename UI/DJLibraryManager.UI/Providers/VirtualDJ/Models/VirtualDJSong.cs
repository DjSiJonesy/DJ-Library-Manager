using System;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Models;

/// <summary>
/// Represents a single song entry in a VirtualDJ database.
/// </summary>
public sealed class VirtualDJSong
{
    public string? FilePath { get; init; }

    public long FileSize { get; init; }

    public string? Author { get; init; }

    public string? Title { get; init; }

    public string? Album { get; init; }

    public string? Genre { get; init; }

    public int? Year { get; init; }

    public double? BPM { get; init; }

    public string? Key { get; init; }

    public TimeSpan Duration { get; init; }

    public DateTime? FirstSeen { get; init; }

    public DateTime? LastModified { get; init; }
}