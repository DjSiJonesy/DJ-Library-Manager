using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Workflow;

using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.ViewModels.Workspace;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Search;

/// <summary>
/// ViewModel for the Search workspace.
///
/// Search investigates issues identified by Analysis and presents
/// possible information or candidates for the user to review.
///
/// Search does not modify the DIASISS library.
/// </summary>
public partial class SearchWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title =>
        "Search";

    public override string Subtitle =>
        "Find information and potential solutions for issues identified during library analysis.";

    // ============================================================
    // Categories
    // ============================================================

    public ObservableCollection<SearchCategoryInfo> Categories { get; }
        = new();

    [ObservableProperty]
    private SearchCategoryInfo? selectedCategory;

    // ============================================================
    // Search Summary
    // ============================================================

    [ObservableProperty]
    private SearchSummary summary = new();

    // ============================================================
    // Search Issues
    // ============================================================

    public ObservableCollection<SearchIssue> Issues { get; }
        = new();

    public ObservableCollection<SearchIssue> FilteredIssues { get; }
        = new();

    [ObservableProperty]
    private SearchIssue? selectedIssue;

    [ObservableProperty]
    private string issueSearchText = string.Empty;

    // ============================================================
    // Search State
    // ============================================================

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private string searchStatus = "Ready";

    // ============================================================
    // Search Progress
    // ============================================================

    /// <summary>
    /// Search All progress for the Duplicates category.
    /// This state is independent from Metadata progress.
    /// </summary>
    [ObservableProperty]
    private double duplicateSearchProgress;

    [ObservableProperty]
    private bool showDuplicateSearchProgress;

    [ObservableProperty]
    private string duplicateSearchProgressText = string.Empty;

    /// <summary>
    /// Search All progress for the Metadata category.
    /// This state is independent from Duplicates progress.
    /// </summary>
    [ObservableProperty]
    private double metadataSearchProgress;

    [ObservableProperty]
    private bool showMetadataSearchProgress;

    [ObservableProperty]
    private string metadataSearchProgressText = string.Empty;

    // ============================================================
    // Category State
    // ============================================================

    /// <summary>
    /// Indicates whether the Duplicates workspace is active.
    /// </summary>
    public bool IsDuplicateCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indicates whether the Metadata workspace is active.
    /// </summary>
    public bool IsMetadataCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Metadata",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indicates whether the Select All Recommended button should
    /// be displayed in the Issues header.
    ///
    /// It is available for Duplicates and Metadata only.
    /// </summary>
    public bool ShowSelectAllRecommended =>
        IsDuplicateCategory ||
        IsMetadataCategory;

    /// <summary>
    /// Indicates whether the selected issue is a Metadata issue.
    /// </summary>
    public bool IsMetadataIssue =>
        SelectedIssue is not null &&
        string.Equals(
            SelectedIssue.Category,
            "Metadata",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Number of currently selected metadata recommendations
    /// for the selected issue.
    /// </summary>
    public int SelectedMetadataChangeCount =>
        SelectedIssue?
            .MetadataRecommendations
            .Count(
                recommendation =>
                    recommendation.IsSelected)
        ?? 0;

    /// <summary>
    /// Number of metadata recommendations that can be selected
    /// for the selected issue.
    /// </summary>
    public int ConfirmableMetadataChangeCount =>
        SelectedIssue?
            .MetadataRecommendations
            .Count(
                recommendation =>
                    recommendation.IsRecommended &&
                    recommendation.IsChange)
        ?? 0;

    public bool HasConfirmableMetadataChanges =>
        ConfirmableMetadataChangeCount > 0;

    /// <summary>
    /// Indicates whether at least one Metadata change has been
    /// selected anywhere in the current Metadata Search workspace.
    ///
    /// This is the visibility condition for the Export Metadata
    /// action.
    ///
    /// The selection is evaluated across all Metadata issues rather
    /// than only the currently selected issue.
    /// </summary>
    public bool HasSelectedMetadataForExport =>
        IsMetadataCategory &&
        Issues
            .Where(
                issue =>
                    string.Equals(
                        issue.Category,
                        "Metadata",
                        StringComparison.OrdinalIgnoreCase))
            .SelectMany(
                issue =>
                    issue.MetadataRecommendations)
            .Any(
                recommendation =>
                    recommendation.IsSelected &&
                    recommendation.IsChange);

    /// <summary>
    /// Indicates whether the current workspace contains selections
    /// made by the bulk Select All Recommended operation.
    ///
    /// User-modified Metadata recommendations are deliberately
    /// excluded from this calculation.
    /// </summary>
    public bool AllRecommendedSelected
    {
        get
        {
            // ----------------------------------------------------
            // Duplicates
            // ----------------------------------------------------

            if (IsDuplicateCategory)
            {
                return Issues.Any(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Duplicates",
                            StringComparison.OrdinalIgnoreCase)
                        &&
                        issue.RecommendedSelectedResultIds.Count > 0);
            }

            // ----------------------------------------------------
            // Metadata
            // ----------------------------------------------------

            if (IsMetadataCategory)
            {
                return Issues
                    .Where(
                        issue =>
                            string.Equals(
                                issue.Category,
                                "Metadata",
                                StringComparison.OrdinalIgnoreCase))
                    .SelectMany(
                        issue =>
                            issue.MetadataRecommendations)
                    .Any(
                        recommendation =>
                            recommendation.IsRecommended &&
                            recommendation.IsChange &&
                            recommendation.IsSelected &&
                            !recommendation.IsUserModified);
            }

            return false;
        }
    }

    // ============================================================
    // Constructor
    // ============================================================

    public SearchWorkspaceViewModel()
    {
        CreateCategories();

        LoadAnalysisSummary();

        App.Services.ApplicationState.AnalysisCompleted +=
            AnalysisCompleted;

        App.Services.Search.ProgressChanged +=
            Search_ProgressChanged;

        RestoreActiveSearchRun();

        UpdateCategorySearchAvailability();
    }

    // ============================================================
    // Categories
    // ============================================================

    private void CreateCategories()
    {
        Categories.Clear();

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Duplicates",
                Icon = "📑",
                Description =
                    "Find duplicate tracks and compare the available copies."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Missing Files",
                Icon = "⚠️",
                Description =
                    "Investigate files that are no longer available at their recorded location."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Metadata",
                Icon = "📋",
                Description =
                    "Find missing or incomplete track metadata. Searching large libraries may take some time."
            });

        SelectedCategory =
            Categories.FirstOrDefault();
    }

    // ============================================================
    // Analysis
    // ============================================================

    private void LoadAnalysisSummary()
    {
        var analysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (analysis is null)
        {
            Summary =
                new SearchSummary();

            foreach (var category in Categories)
            {
                category.IssueCount =
                    0;
            }

            Issues.Clear();

            FilteredIssues.Clear();

            SelectedIssue =
                null;

            SearchStatus =
                "No analysis available";

            UpdateCategorySearchAvailability();

            NotifyMetadataRecommendationProperties();

            return;
        }

        Summary =
            new SearchSummary
            {
                AnalysisDate =
                    analysis.AnalysisDate,

                DuplicateCount =
                    GetIssueCount(
                        analysis,
                        "Duplicates"),

                MissingFileCount =
                    GetIssueCount(
                        analysis,
                        "File Integrity"),

                MetadataIssueCount =
                    GetIssueCount(
                        analysis,
                        "Metadata")
            };

        UpdateCategoryCounts();

        SelectIssuesForCategory();

        SearchStatus =
            Issues.Count == 0
                ? "No issues found"
                : $"{Issues.Count:N0} issues available";

        UpdateCategorySearchAvailability();

        NotifyMetadataRecommendationProperties();
    }

    private void UpdateCategoryCounts()
    {
        SetCategoryCount(
            "Duplicates",
            Summary.DuplicateCount);

        SetCategoryCount(
            "Missing Files",
            Summary.MissingFileCount);

        SetCategoryCount(
            "Metadata",
            Summary.MetadataIssueCount);
    }

    private void SetCategoryCount(
        string categoryName,
        int count)
    {
        var category =
            Categories.FirstOrDefault(
                x =>
                    x.Name.Equals(
                        categoryName,
                        StringComparison.OrdinalIgnoreCase));

        if (category is not null)
        {
            category.IssueCount =
                count;
        }
    }

    private static int GetIssueCount(
        LibraryAnalysisResult analysis,
        string categoryName)
    {
        var category =
            analysis.Categories.FirstOrDefault(
                x =>
                    x.Name.Equals(
                        categoryName,
                        StringComparison.OrdinalIgnoreCase));

        return
            category?.Issues.Count
            ??
            0;
    }

    // ============================================================
    // Category Selection
    // ============================================================

    partial void OnSelectedCategoryChanged(
        SearchCategoryInfo? value)
    {
        IssueSearchText =
            string.Empty;

        SelectIssuesForCategory();

        UpdateSearchRunStatus();

        UpdateCategorySearchAvailability();

        OnPropertyChanged(
            nameof(IsDuplicateCategory));

        OnPropertyChanged(
            nameof(IsMetadataCategory));

        OnPropertyChanged(
            nameof(ShowSelectAllRecommended));

        OnPropertyChanged(
            nameof(AllRecommendedSelected));

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Selected Issue
    // ============================================================

    partial void OnSelectedIssueChanged(
        SearchIssue? value)
    {
        OnPropertyChanged(
            nameof(IsMetadataIssue));

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Issue Search
    // ============================================================

    partial void OnIssueSearchTextChanged(
        string value)
    {
        ApplyIssueFilter();
    }

    private void ApplyIssueFilter()
    {
        FilteredIssues.Clear();

        var filter =
            IssueSearchText?
                .Trim();

        IEnumerable<SearchIssue> filtered;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filtered =
                Issues;
        }
        else
        {
            filtered =
                Issues.Where(
                    issue =>
                    {
                        var text =
                            string.Join(
                                " ",
                                issue.Title,
                                issue.Description,
                                issue.Artist,
                                issue.TrackTitle,
                                issue.FilePath);

                        return text.Contains(
                            filter,
                            StringComparison.OrdinalIgnoreCase);
                    });
        }

        foreach (var issue in
                 filtered
                    .OrderBy(
                        issue => issue.DisplayName,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(
                        issue => issue.FilePath,
                        StringComparer.OrdinalIgnoreCase))
        {
            FilteredIssues.Add(
                issue);
        }
    }

    // ============================================================
    // Category Selection Command
    // ============================================================

    [RelayCommand]
    private void SelectCategory(
        SearchCategoryInfo? category)
    {
        if (category is null)
            return;

        if (ReferenceEquals(
            SelectedCategory,
            category))
        {
            SelectIssuesForCategory();

            UpdateSearchRunStatus();

            UpdateCategorySearchAvailability();

            OnPropertyChanged(
                nameof(IsDuplicateCategory));

            OnPropertyChanged(
                nameof(IsMetadataCategory));

            OnPropertyChanged(
                nameof(ShowSelectAllRecommended));

            OnPropertyChanged(
                nameof(AllRecommendedSelected));

            NotifyMetadataRecommendationProperties();

            return;
        }

        SelectedCategory =
            category;
    }

    // ============================================================
    // Select Issues
    // ============================================================

    private void SelectIssuesForCategory()
    {
        Issues.Clear();

        FilteredIssues.Clear();

        SelectedIssue =
            null;

        if (SelectedCategory is null)
            return;

        var analysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (analysis is null)
            return;

        var analysisCategoryName =
            GetAnalysisCategoryName(
                SelectedCategory.Name);

        if (analysisCategoryName is null)
            return;

        var category =
            analysis.Categories.FirstOrDefault(
                x =>
                    x.Name.Equals(
                        analysisCategoryName,
                        StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        var savedSearch =
            App.Services
                .SearchRepository
                .CurrentSearch;

        var savedIssues =
            savedSearch?
                .Issues
                .ToDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase)
            ??
            new Dictionary<string, SearchIssue>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var analysisIssue in category.Issues)
        {
            var issue =
                CreateSearchIssue(
                    analysisIssue);

            if (savedIssues.TryGetValue(
                issue.Id,
                out var savedIssue))
            {
                RestoreSearchState(
                    issue,
                    savedIssue);
            }

            Issues.Add(
                issue);
        }

        ApplyIssueFilter();

        SearchStatus =
            GetCategorySearchStatus();

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Create Search Issue
    // ============================================================

    private static SearchIssue CreateSearchIssue(
        AnalysisIssue issue)
    {
        var searchIssue =
            new SearchIssue
            {
                Id =
                    issue.Id.ToString(),

                MediaId =
                    issue.MediaId,

                Category =
                    issue.Category,

                Type =
                    issue.Type,

                Title =
                    issue.Title,

                Description =
                    issue.Description,

                Artist =
                    issue.Artist,

                TrackTitle =
                    issue.TrackTitle,

                MissingFields =
                    issue.MissingFields,

                FilenameSearchHint =
                    issue.FilenameSearchHint,

                FilePath =
                    issue.FilePath,

                IsSearched =
                    false,

                HasResults =
                    false
            };

        foreach (var relatedPath in
                 issue.RelatedFilePaths)
        {
            if (string.IsNullOrWhiteSpace(
                relatedPath))
            {
                continue;
            }

            if (string.Equals(
                relatedPath,
                issue.FilePath,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            searchIssue.RelatedFilePaths.Add(
                relatedPath);
        }

        return searchIssue;
    }

    // ============================================================
    // Restore Search State
    // ============================================================

    private static void RestoreSearchState(
        SearchIssue target,
        SearchIssue saved)
    {
        target.IsSearched =
            saved.IsSearched;

        target.HasResults =
            saved.HasResults;

        target.RelatedFilePaths.Clear();

        foreach (var path in
                 saved.RelatedFilePaths)
        {
            if (!string.IsNullOrWhiteSpace(
                path))
            {
                target.RelatedFilePaths.Add(
                    path);
            }
        }

        target.Results.Clear();

        foreach (var result in
                 saved.Results)
        {
            target.Results.Add(
                result);
        }

        target.SelectedResultIds.Clear();

        foreach (var id in
                 saved.SelectedResultIds)
        {
            if (!string.IsNullOrWhiteSpace(
                id))
            {
                target.SelectedResultIds.Add(
                    id);
            }
        }

        target.RecommendedSelectedResultIds.Clear();

        foreach (var id in
                 saved.RecommendedSelectedResultIds)
        {
            if (!string.IsNullOrWhiteSpace(
                id))
            {
                target.RecommendedSelectedResultIds.Add(
                    id);
            }
        }

        target.MetadataRecommendations.Clear();

        foreach (var recommendation in
                 saved.MetadataRecommendations)
        {
            var restored =
                new MetadataChangeRecommendation
                {
                    Field =
                        recommendation.Field,

                    CurrentValue =
                        recommendation.CurrentValue,

                    RecommendedValue =
                        recommendation.RecommendedValue,

                    AgreementPercentage =
                        recommendation.AgreementPercentage,

                    SupportingProviders =
                        recommendation.SupportingProviders,

                    ProvidersWithValue =
                        recommendation.ProvidersWithValue,

                    Strength =
                        recommendation.Strength,

                    IsRecommended =
                        recommendation.IsRecommended,

                    IsSelected =
                        recommendation.IsSelected,

                    Reason =
                        recommendation.Reason
                };

            restored.RestoreSelection(
                recommendation.IsSelected,
                recommendation.IsUserModified);

            target.MetadataRecommendations.Add(
                restored);
        }
    }

    // ============================================================
    // Current Search State
    // ============================================================

    private static SearchState? GetCurrentSearchState(
        LibraryAnalysisResult analysis)
    {
        var savedSearch =
            App.Services
                .SearchRepository
                .CurrentSearch;

        if (savedSearch is null)
            return null;

        if (savedSearch.AnalysisDate !=
            analysis.AnalysisDate)
        {
            return null;
        }

        return savedSearch;
    }

    // ============================================================
    // Save Search State
    // ============================================================

    private void SaveSearchState()
    {
        var analysis =
            App.Services
                .AnalysisRepository
                .CurrentAnalysis;

        if (analysis is null)
            return;

        var existing =
            GetCurrentSearchState(
                analysis);

        var persistedIssues =
            existing?.Issues
                .ToDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase)
            ??
            new Dictionary<string, SearchIssue>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var issue in Issues)
        {
            persistedIssues[issue.Id] =
                issue;
        }

        var state =
            new SearchState
            {
                AnalysisDate =
                    analysis.AnalysisDate,

                SavedAt =
                    DateTime.Now,

                Issues =
                    persistedIssues.Values.ToList()
            };

        App.Services
            .SearchRepository
            .Save(state);
    }

    // ============================================================
    // Search Status
    // ============================================================

    private string GetCategorySearchStatus()
    {
        var searched =
            Issues.Count(
                x => x.IsSearched);

        var withResults =
            Issues.Count(
                x => x.HasResults);

        if (searched == 0)
        {
            return
                $"{Issues.Count:N0} issues available";
        }

        return
            $"{searched:N0} of {Issues.Count:N0} searched • " +
            $"{withResults:N0} with results";
    }

    // ============================================================
    // Category Mapping
    // ============================================================

    private static string? GetAnalysisCategoryName(
        string searchCategory)
    {
        return searchCategory switch
        {
            "Duplicates" =>
                "Duplicates",

            "Missing Files" =>
                "File Integrity",

            "Metadata" =>
                "Metadata",

            _ =>
                null
        };
    }

    // ============================================================
    // Search Individual Issue
    // ============================================================

    [RelayCommand]
    private async Task SearchIssue(
        SearchIssue? issue)
    {
        if (issue is null)
            return;

        if (IsSearching ||
            App.Services.Search.IsSearching)
        {
            return;
        }

        IsSearching =
            true;

        SearchStatus =
            "Searching...";

        try
        {
            var results =
                await App.Services.Search.SearchAsync(
                    issue,
                    CancellationToken.None);

            issue.Results.Clear();

            foreach (var result in results)
            {
                issue.Results.Add(
                    result);
            }

            issue.IsSearched =
                true;

            issue.HasResults =
                issue.Results.Count > 0;

            SaveSearchState();

            // ----------------------------------------------------
            // The metadata recommendations are populated by the
            // Search service. HasSelectedMetadataForExport is a
            // calculated property, so explicitly notify the UI
            // that its underlying state has changed.
            // ----------------------------------------------------

            OnPropertyChanged(
                nameof(AllRecommendedSelected));

            NotifyMetadataRecommendationProperties();

            SearchStatus =
                issue.HasResults
                    ? $"{issue.Results.Count:N0} result(s) found"
                    : "No results found";
        }
        catch
        {
            SearchStatus =
                "Search failed";

            throw;
        }
        finally
        {
            IsSearching =
                false;

            // ----------------------------------------------------
            // Ensure the Export Metadata visibility state is
            // refreshed when an individual search completes.
            // ----------------------------------------------------

            NotifyMetadataRecommendationProperties();
        }
    }

    // ============================================================
    // Search All
    // ============================================================

    [RelayCommand]
    private async Task SearchAll(
        SearchCategoryInfo? category)
    {
        if (category is null)
            return;

        if (IsSearching ||
            App.Services.Search.IsSearching)
        {
            return;
        }

        var analysisCategoryName =
            GetAnalysisCategoryName(
                category.Name);

        if (analysisCategoryName is null)
            return;

        var categoryIssues =
            Issues
                .Where(
                    x =>
                        x.Category.Equals(
                            analysisCategoryName,
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (categoryIssues.Count == 0)
        {
            SearchStatus =
                $"No {category.Name.ToLowerInvariant()} issues found";

            return;
        }

        IsSearching =
            true;

        SetSearchProgressStart(
            category.Name,
            categoryIssues.Count);

        try
        {
            var run =
                await App.Services.Search.SearchAllAsync(
                    categoryIssues,
                    category.Name,
                    CancellationToken.None);

            SearchStatus =
                $"{run.IssuesSearched:N0} " +
                $"{category.Name.ToLowerInvariant()} searched • " +
                $"{run.IssuesWithResults:N0} with results";
        }
        finally
        {
            IsSearching =
                false;

            UpdateCategorySearchAvailability();

            OnPropertyChanged(
                nameof(AllRecommendedSelected));

            NotifyMetadataRecommendationProperties();
        }
    }

    // ============================================================
    // Select All Recommended
    // ============================================================

    [RelayCommand]
    private void SelectAllRecommended()
    {
        if (IsSearching ||
            App.Services.Search.IsSearching)
        {
            return;
        }

        if (SelectedCategory is null)
            return;

        // ========================================================
        // Metadata
        // ========================================================

        if (IsMetadataCategory)
        {
            SelectAllRecommendedMetadata();

            return;
        }

        // ========================================================
        // Duplicates
        // ========================================================

        if (!IsDuplicateCategory)
            return;

        var hasBulkSelections =
            Issues.Any(
                issue =>
                    string.Equals(
                        issue.Category,
                        "Duplicates",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    issue.RecommendedSelectedResultIds.Count > 0);

        // --------------------------------------------------------
        // Toggle OFF bulk duplicate selections.
        //
        // Existing duplicate behaviour is preserved.
        // --------------------------------------------------------

        if (hasBulkSelections)
        {
            var changed =
                false;

            foreach (var issue in Issues)
            {
                if (!string.Equals(
                    issue.Category,
                    "Duplicates",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (issue.RecommendedSelectedResultIds.Count == 0)
                    continue;

                var bulkIds =
                    issue.RecommendedSelectedResultIds.ToList();

                foreach (var resultId in bulkIds)
                {
                    var result =
                        issue.Results.FirstOrDefault(
                            x =>
                                string.Equals(
                                    x.Id,
                                    resultId,
                                    StringComparison.OrdinalIgnoreCase));

                    if (result is not null)
                    {
                        result.IsSelected =
                            false;
                    }

                    RemoveId(
                        issue.SelectedResultIds,
                        resultId);
                }

                issue.RecommendedSelectedResultIds.Clear();

                changed =
                    true;
            }

            if (changed)
            {
                SaveSearchState();
            }

            OnPropertyChanged(
                nameof(AllRecommendedSelected));

            return;
        }

        // --------------------------------------------------------
        // Select duplicate recommendations.
        // --------------------------------------------------------

        var selectionChanged =
            false;

        foreach (var issue in Issues)
        {
            if (!string.Equals(
                issue.Category,
                "Duplicates",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (issue.SelectedResultIds.Count > 0)
                continue;

            var recommended =
                issue.Results.FirstOrDefault(
                    result =>
                        result.IsRecommended);

            if (recommended is null)
                continue;

            recommended.IsSelected =
                true;

            AddId(
                issue.SelectedResultIds,
                recommended.Id);

            AddId(
                issue.RecommendedSelectedResultIds,
                recommended.Id);

            selectionChanged =
                true;
        }

        if (selectionChanged)
        {
            SaveSearchState();
        }

        OnPropertyChanged(
            nameof(AllRecommendedSelected));
    }

    // ============================================================
    // Metadata Bulk Selection
    // ============================================================

    private void SelectAllRecommendedMetadata()
    {
        var metadataIssues =
            Issues
                .Where(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Metadata",
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (metadataIssues.Count == 0)
            return;

        // --------------------------------------------------------
        // If an automatic bulk selection currently exists,
        // clicking the button clears ONLY those automatic
        // selections.
        //
        // Anything the user has explicitly changed remains
        // untouched.
        // --------------------------------------------------------

        var bulkSelectionsExist =
            metadataIssues
                .SelectMany(
                    issue =>
                        issue.MetadataRecommendations)
                .Any(
                    recommendation =>
                        recommendation.IsRecommended &&
                        recommendation.IsChange &&
                        recommendation.IsSelected &&
                        !recommendation.IsUserModified);

        if (bulkSelectionsExist)
        {
            foreach (var issue in metadataIssues)
            {
                foreach (var recommendation in
                         issue.MetadataRecommendations)
                {
                    if (!recommendation.IsRecommended)
                        continue;

                    if (!recommendation.IsChange)
                        continue;

                    if (!recommendation.IsSelected)
                        continue;

                    if (recommendation.IsUserModified)
                        continue;

                    recommendation.SetRecommendedSelection(
                        false);
                }
            }

            SaveSearchState();

            NotifyMetadataRecommendationProperties();

            return;
        }

        // --------------------------------------------------------
        // Select all valid recommendations.
        //
        // IsRecommended is authoritative.
        //
        // User-modified recommendations are never changed.
        // --------------------------------------------------------

        foreach (var issue in metadataIssues)
        {
            foreach (var recommendation in
                     issue.MetadataRecommendations)
            {
                if (!recommendation.IsRecommended)
                    continue;

                if (!recommendation.IsChange)
                    continue;

                if (recommendation.IsUserModified)
                    continue;

                recommendation.SetRecommendedSelection(
                    true);
            }
        }

        SaveSearchState();

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Metadata Select All Recommended Command
    // ============================================================

    /// <summary>
    /// Selects all recommended Metadata changes without invoking
    /// the Duplicates bulk-selection command.
    ///
    /// User-modified Metadata recommendations remain untouched.
    /// </summary>
    [RelayCommand]
    private void SelectAllRecommendedMetadataOnly()
    {
        if (IsSearching ||
            App.Services.Search.IsSearching)
        {
            return;
        }

        if (!IsMetadataCategory)
            return;

        SelectAllRecommendedMetadata();
    }

    // ============================================================
    // Export Metadata
    // ============================================================

    /// <summary>
    /// Exports only the Metadata changes currently selected by the
    /// user.
    ///
    /// SearchExportService is responsible for extracting the selected
    /// metadata recommendations from the supplied SearchIssue objects.
    /// The ViewModel therefore passes SearchIssue objects directly and
    /// does not create a second export-specific selection model.
    /// </summary>
    [RelayCommand]
    private async Task ExportMetadata()
    {
        if (IsSearching ||
            App.Services.Search.IsSearching)
        {
            return;
        }

        if (!IsMetadataCategory)
        {
            return;
        }

        var metadataIssues =
            Issues
                .Where(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Metadata",
                            StringComparison.OrdinalIgnoreCase))
                .Where(
                    issue =>
                        issue.MetadataRecommendations.Any(
                            recommendation =>
                                recommendation.IsSelected &&
                                recommendation.IsChange))
                .ToList();

        if (metadataIssues.Count == 0)
        {
            SearchStatus =
                "No selected metadata changes to export";

            NotifyMetadataRecommendationProperties();

            return;
        }

        var application =
            Avalonia.Application.Current;

        if (application?.ApplicationLifetime
            is not Avalonia.Controls.ApplicationLifetimes
                .IClassicDesktopStyleApplicationLifetime desktop)
        {
            SearchStatus =
                "Unable to access application window";

            return;
        }

        var topLevel =
            Avalonia.Controls.TopLevel.GetTopLevel(
                desktop.MainWindow);

        if (topLevel is null)
        {
            SearchStatus =
                "Unable to open Save File dialog";

            return;
        }

        var storageProvider =
            topLevel.StorageProvider;

        var file =
            await storageProvider.SaveFilePickerAsync(
                new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title =
                        "Export Selected Metadata",

                    SuggestedFileName =
                        "DIASISS_Selected_Metadata",

                    DefaultExtension =
                        "xlsx",

                    FileTypeChoices =
                        new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType(
                                "Excel Workbook")
                            {
                                Patterns =
                                    new[]
                                    {
                                        "*.xlsx"
                                    }
                            },

                            new Avalonia.Platform.Storage.FilePickerFileType(
                                "CSV File")
                            {
                                Patterns =
                                    new[]
                                    {
                                        "*.csv"
                                    }
                            },

                            new Avalonia.Platform.Storage.FilePickerFileType(
                                "JSON File")
                            {
                                Patterns =
                                    new[]
                                    {
                                        "*.json"
                                    }
                            }
                        }
                });

        if (file is null)
        {
            return;
        }

        var filePath =
            file.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            SearchStatus =
                "Unable to access selected file";

            return;
        }

        try
        {
            var extension =
                System.IO.Path
                    .GetExtension(filePath)
                    .ToLowerInvariant();

            switch (extension)
            {
                case ".xlsx":

                    await App.Services
                        .SearchExportService
                        .ExportSelectedMetadataXlsxAsync(
                            metadataIssues,
                            filePath);

                    break;

                case ".csv":

                    await App.Services
                        .SearchExportService
                        .ExportSelectedMetadataCsvAsync(
                            metadataIssues,
                            filePath);

                    break;

                case ".json":

                    await App.Services
                        .SearchExportService
                        .ExportSelectedMetadataJsonAsync(
                            metadataIssues,
                            filePath);

                    break;

                default:

                    SearchStatus =
                        "Unsupported export file type";

                    return;
            }

            SearchStatus =
                $"{metadataIssues.Sum(issue => issue.MetadataRecommendations.Count(
                    recommendation =>
                        recommendation.IsSelected &&
                        recommendation.IsChange)):N0} metadata changes exported";
        }
        catch
        {
            SearchStatus =
                "Metadata export failed";

            throw;
        }
    }

    /// <summary>
    /// Represents one metadata recommendation selected for export.
    ///
    /// The SearchIssue is retained alongside the recommendation so
    /// the export service can identify the affected track.
    /// </summary>
    private sealed record SelectedMetadataExportItem(
        SearchIssue Issue,
        MetadataChangeRecommendation Recommendation);

    // ============================================================
    // Metadata Property Notifications
    // ============================================================

    private void NotifyMetadataRecommendationProperties()
    {
        OnPropertyChanged(
            nameof(AllRecommendedSelected));

        OnPropertyChanged(
            nameof(SelectedMetadataChangeCount));

        OnPropertyChanged(
            nameof(ConfirmableMetadataChangeCount));

        OnPropertyChanged(
            nameof(HasConfirmableMetadataChanges));

        OnPropertyChanged(
            nameof(HasSelectedMetadataForExport));
    }

    // ============================================================
    // Duplicate Selection Helpers
    // ============================================================

    private static void AddId(
        IList<string> ids,
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (ids.Any(
            x =>
                string.Equals(
                    x,
                    id,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        ids.Add(
            id);
    }

    private static void RemoveId(
        IList<string> ids,
        string id)
    {
        for (var index = ids.Count - 1;
             index >= 0;
             index--)
        {
            if (string.Equals(
                ids[index],
                id,
                StringComparison.OrdinalIgnoreCase))
            {
                ids.RemoveAt(
                    index);
            }
        }
    }

    // ============================================================
    // Select Result
    // ============================================================

    /// <summary>
    /// Selects a duplicate SearchResult.
    ///
    /// SearchIssue.SelectedResultIds is the authoritative
    /// duplicate selection state.
    /// </summary>
    public void SelectResult(
        SearchResult? result)
    {
        if (SelectedIssue is null ||
            result is null)
        {
            return;
        }

        foreach (var issueResult in
                 SelectedIssue.Results)
        {
            issueResult.IsSelected =
                ReferenceEquals(
                    issueResult,
                    result);
        }

        SelectedIssue.SelectedResultIds.Clear();

        if (!string.IsNullOrWhiteSpace(
            result.Id))
        {
            SelectedIssue.SelectedResultIds.Add(
                result.Id);
        }

        SelectedIssue.RecommendedSelectedResultIds.Clear();

        SaveSearchState();
    }

    // ============================================================
    // Search Run
    // ============================================================

    private void RestoreActiveSearchRun()
    {
        var run =
            App.Services.Search.CurrentRun;

        if (run is null)
            return;

        var analysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (analysis is null)
            return;

        if (run.AnalysisDate !=
            analysis.AnalysisDate)
        {
            return;
        }

        var category =
            Categories.FirstOrDefault(
                x =>
                    x.Name.Equals(
                        run.Category,
                        StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        SelectedCategory =
            category;

        IsSearching =
            App.Services.Search.IsSearching;

        UpdateSearchProgress(
            run);

        UpdateSearchRunStatus();

        NotifyMetadataRecommendationProperties();
    }

    private void SetSearchProgressStart(
        string categoryName,
        int totalIssues)
    {
        var text =
            $"0 of {totalIssues:N0} {categoryName.ToLowerInvariant()} searched";

        if (string.Equals(
            categoryName,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase))
        {
            DuplicateSearchProgress = 0;
            DuplicateSearchProgressText = text;
            ShowDuplicateSearchProgress = true;
            return;
        }

        if (string.Equals(
            categoryName,
            "Metadata",
            StringComparison.OrdinalIgnoreCase))
        {
            MetadataSearchProgress = 0;
            MetadataSearchProgressText = text;
            ShowMetadataSearchProgress = true;
        }
    }

    private void UpdateSearchProgress(
        SearchRun run)
    {
        var total =
            Math.Max(
                run.TotalIssues,
                0);

        var searched =
            Math.Clamp(
                run.IssuesSearched,
                0,
                total);

        var progress =
            total == 0
                ? 0
                : searched * 100d / total;

        var text =
            total == 0
                ? string.Empty
                : $"{searched:N0} of {total:N0} {run.Category.ToLowerInvariant()} searched";

        if (string.Equals(
            run.Category,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase))
        {
            DuplicateSearchProgress = progress;
            DuplicateSearchProgressText = text;
            ShowDuplicateSearchProgress =
                run.Status == "Running" ||
                run.Status == "Completed";
            return;
        }

        if (string.Equals(
            run.Category,
            "Metadata",
            StringComparison.OrdinalIgnoreCase))
        {
            MetadataSearchProgress = progress;
            MetadataSearchProgressText = text;
            ShowMetadataSearchProgress =
                run.Status == "Running" ||
                run.Status == "Completed";
        }
    }

    private void UpdateSearchRunStatus()
    {
        var run =
            App.Services.Search.CurrentRun;

        if (run is null)
            return;

        if (SelectedCategory is null)
            return;

        if (!run.Category.Equals(
            SelectedCategory.Name,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        IsSearching =
            App.Services.Search.IsSearching;

        if (run.Status == "Running")
        {
            SearchStatus =
                $"Searching {run.IssuesSearched:N0} of " +
                $"{run.TotalIssues:N0} " +
                $"{run.Category.ToLowerInvariant()}...";
        }
        else if (run.Status == "Completed")
        {
            SearchStatus =
                $"{run.IssuesSearched:N0} " +
                $"{run.Category.ToLowerInvariant()} searched • " +
                $"{run.IssuesWithResults:N0} with results";
        }

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Category Search Availability
    // ============================================================

    private void UpdateCategorySearchAvailability()
    {
        var searchService =
            App.Services.Search;

        var searchRunning =
            searchService.IsSearching;

        var runningCategory =
            searchService.CurrentRun?.Category;

        foreach (var category in Categories)
        {
            if (!searchRunning)
            {
                category.IsSearchEnabled =
                    true;

                continue;
            }

            category.IsSearchEnabled =
                string.Equals(
                    category.Name,
                    runningCategory,
                    StringComparison.OrdinalIgnoreCase);
        }
    }

    // ============================================================
    // Search Progress
    // ============================================================

    private void Search_ProgressChanged(
        object? sender,
        SearchRun run)
    {
        if (run is null)
            return;

        var analysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (analysis is null)
            return;

        if (run.AnalysisDate !=
            analysis.AnalysisDate)
        {
            return;
        }

        UpdateCategorySearchAvailability();

        var category =
            Categories.FirstOrDefault(
                x =>
                    x.Name.Equals(
                        run.Category,
                        StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        if (!ReferenceEquals(
            SelectedCategory,
            category))
        {
            SelectedCategory =
                category;
        }

        IsSearching =
            App.Services.Search.IsSearching;

        UpdateSearchProgress(
            run);

        if (run.Status == "Running")
        {
            SearchStatus =
                $"Searching {run.IssuesSearched:N0} of " +
                $"{run.TotalIssues:N0} " +
                $"{run.Category.ToLowerInvariant()}...";
        }
        else if (run.Status == "Completed")
        {
            SearchStatus =
                $"{run.IssuesSearched:N0} " +
                $"{run.Category.ToLowerInvariant()} searched • " +
                $"{run.IssuesWithResults:N0} with results";

            IsSearching =
                false;
        }

        OnPropertyChanged(
            nameof(AllRecommendedSelected));

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Analysis Completed
    // ============================================================

    private void AnalysisCompleted(
        object? sender,
        EventArgs e)
    {
        LoadAnalysisSummary();
    }

    // ============================================================
    // Workflow Navigation
    // ============================================================

    [RelayCommand]
    private void Previous()
    {
        App.Services.ApplicationState.NavigateTo(
            WorkspaceType.Analysis);
    }

    [RelayCommand]
    private void Next()
    {
        App.Services.ApplicationState.NavigateTo(
            WorkspaceType.Improve);
    }
}