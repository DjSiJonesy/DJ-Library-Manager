using System;

namespace DJLibraryManager.UI.Models.Media;

/// <summary>
/// Represents a provider-independent media item within DIASISS.
/// Every supported DJ platform is converted into this common model.
///
/// MediaId is the DIASISS identity of the physical/logical track record.
/// TrackStatusId represents the current lifecycle state of the track:
///
/// 1  = Good
/// 2  = Missing or Corrupt
/// 99 = Removed
///
/// Provider-specific identity information is stored separately from
/// this model in MediaProviderIdentities.
/// </summary>
public sealed class DJLMMediaItem
{
    /// <summary>
    /// The DIASISS GUID identifying this media record.
    /// </summary>
    public string MediaId { get; set; } = string.Empty;

    /// <summary>
    /// Current DIASISS track status.
    ///
    /// 1  = Good
    /// 2  = Missing or Corrupt
    /// 99 = Removed
    /// </summary>
    public int TrackStatusId { get; set; } = 1;

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
    /// Date the track was first seen by the provider.
    /// </summary>
    public DateTime? DateFirstSeen { get; set; }

    /// <summary>
    /// Date the track was last modified in the provider.
    /// </summary>
    public DateTime? DateLastModified { get; set; }

    /// <summary>
    /// Date the DIASISS media record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date the DIASISS media record was last modified.
    /// </summary>
    public DateTime LastModifiedDate { get; set; }
}