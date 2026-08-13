using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents an individual issue that the Search workflow
/// needs to investigate.
///
/// Search does not modify the DIASISS library. It uses this
/// information to locate possible solutions or candidates.
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
    // Media
    // ============================================================

    [ObservableProperty]
    private string artist = string.Empty;

    [ObservableProperty]
    private string trackTitle = string.Empty;

    [ObservableProperty]
    private string filePath = string.Empty;

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
    /// can contain legitimate versions of the same track, such as:
    ///
    /// - Album / studio version
    /// - Live version
    /// - Remix
    /// - Radio edit
    /// - Acoustic version
    /// - Instrumental version
    ///
    /// This is the persisted source of truth for user selections.
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
    /// This is retained temporarily so previously persisted Search
    /// state containing SelectedResultId can still be loaded.
    ///
    /// New code should use SelectedResultIds.
    /// </summary>
    [ObservableProperty]
    private string? selectedResultId;

    /// <summary>
    /// Indicates whether the current legacy selection was applied
    /// by the Select All Recommended action.
    ///
    /// Retained for backwards compatibility with previously
    /// persisted Search state.
    ///
    /// New code should use RecommendedSelectedResultIds.
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
}