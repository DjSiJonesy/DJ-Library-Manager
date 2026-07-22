using System;

namespace DJLibraryManager.UI.Models.Media;

/// <summary>
/// Represents a provider-independent media item within DJ Library Manager.
/// Every supported DJ platform is converted into this common model.
/// </summary>
public sealed class DJLMMediaItem
{
    /// <summary>
    /// The provider this media item originated from.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Audio or Video.
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Full file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Artist.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Track title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Album.
    /// </summary>
    public string Album { get; set; } = string.Empty;

    /// <summary>
    /// Genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Year.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// BPM.
    /// </summary>
    public double? BPM { get; set; }

    /// <summary>
    /// Musical key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Duration.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Date first seen by the provider.
    /// </summary>
    public DateTime? DateFirstSeen { get; set; }

    /// <summary>
    /// Date last modified in the provider.
    /// </summary>
    public DateTime? DateLastModified { get; set; }
}