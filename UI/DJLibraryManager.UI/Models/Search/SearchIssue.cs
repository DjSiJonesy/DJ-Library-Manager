using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Search.Models;
using System.Collections.Generic;

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents an individual issue that the Search workflow
/// needs to investigate.
///
/// Search does not modify the DIASISS library. It uses this
/// information to locate possible solutions or candidates.
///
/// The issue also contains the metadata currently known by
/// DIASISS so that Search can clearly distinguish existing
/// metadata from newly discovered metadata.
/// </summary>
public partial class SearchIssue : ObservableObject
{
    // ============================================================
    // Identity
    // ============================================================

    [ObservableProperty]
    private string id = string.Empty;

    // ============================================================
    // Category
    // ============================================================

    [ObservableProperty]
    private string category = string.Empty;

    // ============================================================
    // Issue
    // ============================================================

    [ObservableProperty]
    private string type = string.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    // ============================================================
    // Existing Metadata
    // ============================================================

    /// <summary>
    /// Artist currently stored in the DIASISS library.
    ///
    /// This is actual library metadata and must not be replaced
    /// with a filename-derived search value.
    /// </summary>
    [ObservableProperty]
    private string artist = string.Empty;

    /// <summary>
    /// Track title currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private string trackTitle = string.Empty;

    /// <summary>
    /// Album currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private string album = string.Empty;

    /// <summary>
    /// Genre currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private string genre = string.Empty;

    /// <summary>
    /// Year currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private int? year;

    /// <summary>
    /// BPM currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private double? bpm;

    /// <summary>
    /// Musical key currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private string key = string.Empty;

    /// <summary>
    /// Duration currently stored in the DIASISS library.
    /// </summary>
    [ObservableProperty]
    private TimeSpan? duration;

    // ============================================================
    // Search Information
    // ============================================================

    /// <summary>
    /// Search information derived from the physical filename.
    ///
    /// These are search hints only. They are not confirmed Artist
    /// or Title metadata.
    ///
    /// FilenameSearchHint may contain multiple possible
    /// Artist/Title interpretations because filenames can use
    /// different conventions, such as:
    ///
    ///     Artist - Title
    ///     Title - Artist
    /// </summary>
    public FilenameSearchHint? FilenameSearchHint { get; set; }

    /// <summary>
    /// Physical file path of the affected track.
    /// </summary>
    [ObservableProperty]
    private string filePath = string.Empty;

    // ============================================================
    // Display Name
    // ============================================================

    /// <summary>
    /// Human-readable name used by the Search issue list.
    ///
    /// The display follows this priority:
    ///
    /// 1. Artist + Title
    /// 2. Title
    /// 3. Artist
    /// 4. Filename without extension
    /// 5. Unknown Track
    ///
    /// Filename-derived information is only used as a display
    /// fallback. It does not populate Artist or TrackTitle.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var currentArtist =
                Artist?.Trim() ?? string.Empty;

            var currentTitle =
                TrackTitle?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(currentArtist) &&
                !string.IsNullOrWhiteSpace(currentTitle))
            {
                return $"{currentArtist} — {currentTitle}";
            }

            if (!string.IsNullOrWhiteSpace(currentTitle))
            {
                return currentTitle;
            }

            if (!string.IsNullOrWhiteSpace(currentArtist))
            {
                return currentArtist;
            }

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                try
                {
                    var filename =
                        Path.GetFileNameWithoutExtension(
                            FilePath);

                    if (!string.IsNullOrWhiteSpace(filename))
                    {
                        return filename;
                    }
                }
                catch
                {
                    // Fall through to Unknown Track.
                }
            }

            return "Unknown Track";
        }
    }

    /// <summary>
    /// Filename of the affected file without its extension.
    ///
    /// Useful when Artist and Title are missing and the filename
    /// is being used as a search hint.
    /// </summary>
    public string FilenameDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return string.Empty;

            try
            {
                return Path.GetFileNameWithoutExtension(
                    FilePath);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    // ============================================================
    // Related Files
    // ============================================================

    /// <summary>
    /// Other files belonging to the same issue.
    ///
    /// For duplicate issues this contains the other files
    /// identified as part of the duplicate group.
    /// </summary>
    public ObservableCollection<string> RelatedFilePaths { get; }
        = new();

    /// <summary>
    /// Total number of files in the duplicate group.
    ///
    /// RelatedFilePaths contains the other copies, while the
    /// current issue represents the first copy.
    /// </summary>
    public int CopyCount =>
        RelatedFilePaths.Count + 1;

    // ============================================================
    // Search State
    // ============================================================

    [ObservableProperty]
    private bool isSearched;

    [ObservableProperty]
    private bool hasResults;

    // ============================================================
    // Duplicate Selection
    // ============================================================

    /// <summary>
    /// IDs of all SearchResults the user has chosen to keep.
    ///
    /// Multiple results may be selected because a duplicate group
    /// can contain legitimate versions of the same track.
    /// </summary>
    public ObservableCollection<string> SelectedResultIds { get; }
        = new();

    /// <summary>
    /// IDs of results that were selected by the Select All
    /// Recommended action.
    ///
    /// This allows the bulk action to remove only the selections
    /// that it created without touching selections made manually
    /// by the user.
    /// </summary>
    public ObservableCollection<string> RecommendedSelectedResultIds { get; }
        = new();

    /// <summary>
    /// Legacy single-selection property.
    ///
    /// Retained temporarily so previously persisted Search state
    /// containing SelectedResultId can still be loaded.
    /// </summary>
    [ObservableProperty]
    private string? selectedResultId;

    /// <summary>
    /// Indicates whether the current legacy selection was applied
    /// by the Select All Recommended action.
    /// </summary>
    [ObservableProperty]
    private bool selectionWasRecommended;

    // ============================================================
    // Search Results
    // ============================================================

    /// <summary>
    /// Possible solutions or candidates discovered by Search.
    ///
    /// Observable so the Search Workspace updates immediately
    /// when a search completes.
    /// </summary>
    public ObservableCollection<SearchResult> Results { get; }
        = new();

    // ============================================================
    // Metadata Recommendations
    // ============================================================

    /// <summary>
    /// Metadata changes discovered by the Search workflow.
    ///
    /// Each item represents a proposed change only. Selecting an
    /// item does not modify the physical file or DIASISS library.
    /// </summary>
    public ObservableCollection<MetadataChangeRecommendation>
        MetadataRecommendations
    { get; }
        = new();

    /// <summary>
    /// Metadata fields identified by Analysis as missing from the
    /// affected track.
    ///
    /// Search uses this as the authoritative list of metadata fields
    /// that need investigation.
    ///
    /// These values come from Analysis. Search must not attempt to
    /// reconstruct the missing fields from the issue Type or title.
    /// </summary>
    public IReadOnlyList<string> MissingFields { get; set; }
        = Array.Empty<string>();

    // ============================================================
    // Property Change Notifications
    // ============================================================

    partial void OnArtistChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnTrackTitleChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnFilePathChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(FilenameDisplay));
    }
}