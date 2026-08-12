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

    // ============================================================
    // Search State
    // ============================================================

    [ObservableProperty]
    private bool isSearched;

    [ObservableProperty]
    private bool hasResults;

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