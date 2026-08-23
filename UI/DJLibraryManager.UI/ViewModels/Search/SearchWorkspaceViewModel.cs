using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.ViewModels.Workspace;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
///
/// Long-running Search All operations are owned by SearchService
/// rather than this ViewModel so they continue while the user
/// navigates between workspaces.
/// </summary>
public partial class SearchWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title => "Search";

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

    // ============================================================
    // Issue Filtering
    // ============================================================

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
    // Metadata Recommendation Threshold
    // ============================================================

    /// <summary>
    /// Minimum confidence required for the bulk
    /// "Confirm Recommended Changes" action.
    ///
    /// This controls UI selection only.
    /// It does not itself modify the library.
    /// </summary>
    [ObservableProperty]
    private double metadataRecommendationThreshold = 90.0;

    /// <summary>
    /// Indicates whether the selected metadata issue has at
    /// least one recommended change meeting the configured
    /// confirmation threshold.
    /// </summary>
    public bool HasConfirmableMetadataChanges =>
        SelectedIssue?
            .MetadataRecommendations
            .Any(
                recommendation =>
                    recommendation.IsRecommended &&
                    recommendation.IsChange &&
                    recommendation.AgreementPercentage >=
                        MetadataRecommendationThreshold)
        ?? false;

    /// <summary>
    /// Indicates whether the currently selected issue is a
    /// metadata issue.
    /// </summary>
    public bool IsMetadataIssue =>
        string.Equals(
            SelectedIssue?.Category,
            "Metadata",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Number of metadata changes currently selected for the
    /// selected issue.
    /// </summary>
    public int SelectedMetadataChangeCount =>
        SelectedIssue?
            .MetadataRecommendations
            .Count(
                x => x.IsSelected)
        ?? 0;

    /// <summary>
    /// Number of metadata changes that DIASISS recommends and
    /// which meet the configured bulk confirmation threshold.
    /// </summary>
    public int ConfirmableMetadataChangeCount =>
        SelectedIssue?
            .MetadataRecommendations
            .Count(
                recommendation =>
                    recommendation.IsRecommended &&
                    recommendation.IsChange &&
                    recommendation.AgreementPercentage >=
                        MetadataRecommendationThreshold)
        ?? 0;

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
                    "Find missing or incomplete track metadata."
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
            Summary = new SearchSummary();

            foreach (var category in Categories)
            {
                category.IssueCount = 0;
            }

            Issues.Clear();

            FilteredIssues.Clear();

            SelectedIssue = null;

            SearchStatus =
                "No analysis available";

            UpdateCategorySearchAvailability();

            return;
        }

        Summary = new SearchSummary
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
                x => x.Name.Equals(
                    categoryName,
                    StringComparison.OrdinalIgnoreCase));

        if (category is not null)
        {
            category.IssueCount = count;
        }
    }

    // ============================================================
    // Category Selection
    // ============================================================

    partial void OnSelectedCategoryChanged(
        SearchCategoryInfo? value)
    {
        IssueSearchText = string.Empty;

        SelectIssuesForCategory();

        UpdateSearchRunStatus();

        UpdateCategorySearchAvailability();

        OnPropertyChanged(
            nameof(IsDuplicateCategory));

        OnPropertyChanged(
            nameof(IsMetadataIssue));

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

    private void NotifyMetadataRecommendationProperties()
    {
        OnPropertyChanged(
            nameof(HasConfirmableMetadataChanges));

        OnPropertyChanged(
            nameof(SelectedMetadataChangeCount));

        OnPropertyChanged(
            nameof(ConfirmableMetadataChangeCount));
    }

    // ============================================================
    // Issue Search Text
    // ============================================================

    partial void OnIssueSearchTextChanged(
        string value)
    {
        FilterIssues();
    }

    // ============================================================
    // Issue Filtering
    // ============================================================

    private void FilterIssues()
    {
        var previousSelectedId =
            SelectedIssue?.Id;

        FilteredIssues.Clear();

        var searchText =
            IssueSearchText?.Trim();

        IEnumerable<SearchIssue> filtered =
            Issues;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered =
                Issues.Where(
                    issue =>
                        ContainsIgnoreCase(
                            issue.DisplayName,
                            searchText)
                        ||
                        ContainsIgnoreCase(
                            issue.Artist,
                            searchText)
                        ||
                        ContainsIgnoreCase(
                            issue.TrackTitle,
                            searchText)
                        ||
                        ContainsIgnoreCase(
                            issue.Album,
                            searchText)
                        ||
                        ContainsIgnoreCase(
                            issue.Description,
                            searchText)
                        ||
                        ContainsIgnoreCase(
                            issue.FilePath,
                            searchText));
        }

        var sorted =
            filtered
                .OrderBy(
                    x => x.DisplayName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    x => x.FilePath,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        foreach (var issue in sorted)
        {
            FilteredIssues.Add(issue);
        }

        if (!string.IsNullOrWhiteSpace(previousSelectedId))
        {
            var previouslySelected =
                FilteredIssues.FirstOrDefault(
                    x => string.Equals(
                        x.Id,
                        previousSelectedId,
                        StringComparison.OrdinalIgnoreCase));

            if (previouslySelected is not null)
            {
                SelectedIssue =
                    previouslySelected;

                return;
            }
        }

        SelectedIssue =
            FilteredIssues.FirstOrDefault();
    }

    private static bool ContainsIgnoreCase(
        string? value,
        string searchText)
    {
        return !string.IsNullOrWhiteSpace(value)
            &&
            value.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase);
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
            IssueSearchText = string.Empty;

            SelectIssuesForCategory();

            UpdateSearchRunStatus();

            UpdateCategorySearchAvailability();

            OnPropertyChanged(
                nameof(IsDuplicateCategory));

            OnPropertyChanged(
                nameof(IsMetadataIssue));

            OnPropertyChanged(
                nameof(AllRecommendedSelected));

            NotifyMetadataRecommendationProperties();

            return;
        }

        SelectedCategory =
            category;
    }

    // ============================================================
    // Select Issues For Category
    // ============================================================

    private void SelectIssuesForCategory()
    {
        Issues.Clear();

        FilteredIssues.Clear();

        SelectedIssue = null;

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
                x => x.Name.Equals(
                    analysisCategoryName,
                    StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        var savedSearch =
            GetCurrentSearchState(
                analysis);

        var savedIssues =
            savedSearch?
                .Issues
                .ToDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, SearchIssue>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var issue in category.Issues)
        {
            var searchIssue =
                CreateSearchIssue(issue);

            if (savedIssues.TryGetValue(
                    searchIssue.Id,
                    out var savedIssue))
            {
                RestoreSearchState(
                    searchIssue,
                    savedIssue);
            }

            Issues.Add(
                searchIssue);
        }

        FilterIssues();

        SearchStatus =
            Issues.Count == 0
                ? "No issues found"
                : GetCategorySearchStatus();

        OnPropertyChanged(
            nameof(AllRecommendedSelected));

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

                Album =
                    issue.Album,

                Genre =
                    issue.Genre,

                Year =
                    issue.Year,

                Bpm =
                    issue.BPM,

                Key =
                    issue.Key,

                Duration =
                    issue.Duration,

                // ------------------------------------------------
                // Preserve Analysis' authoritative missing fields.
                //
                // Analysis determines what is actually missing.
                // Search must not reconstruct this information
                // from the issue description or issue type.
                // ------------------------------------------------

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

        target.SelectedResultIds.Clear();

        foreach (var resultId in
                 saved.SelectedResultIds)
        {
            if (!string.IsNullOrWhiteSpace(resultId) &&
                !target.SelectedResultIds.Contains(
                    resultId,
                    StringComparer.OrdinalIgnoreCase))
            {
                target.SelectedResultIds.Add(
                    resultId);
            }
        }

        target.RecommendedSelectedResultIds.Clear();

        foreach (var resultId in
                 saved.RecommendedSelectedResultIds)
        {
            if (!string.IsNullOrWhiteSpace(resultId) &&
                !target.RecommendedSelectedResultIds.Contains(
                    resultId,
                    StringComparer.OrdinalIgnoreCase))
            {
                target.RecommendedSelectedResultIds.Add(
                    resultId);
            }
        }

        if (target.SelectedResultIds.Count == 0 &&
            !string.IsNullOrWhiteSpace(
                saved.SelectedResultId))
        {
            target.SelectedResultIds.Add(
                saved.SelectedResultId);
        }

        if (target.RecommendedSelectedResultIds.Count == 0 &&
            saved.SelectionWasRecommended &&
            !string.IsNullOrWhiteSpace(
                saved.SelectedResultId))
        {
            target.RecommendedSelectedResultIds.Add(
                saved.SelectedResultId);
        }

        SyncLegacySelectionState(
            target);

        target.RelatedFilePaths.Clear();

        foreach (var path in
                 saved.RelatedFilePaths)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                target.RelatedFilePaths.Add(
                    path);
            }
        }

        target.Results.Clear();

        foreach (var result in
                 saved.Results)
        {
            result.IsSelected =
                target.SelectedResultIds.Contains(
                    result.Id,
                    StringComparer.OrdinalIgnoreCase);

            target.Results.Add(
                result);
        }

        // --------------------------------------------------------
        // Restore metadata recommendations.
        //
        // These are independent of SearchResult and therefore
        // must be restored separately.
        // --------------------------------------------------------

        target.MetadataRecommendations.Clear();

        foreach (var recommendation in
                 saved.MetadataRecommendations)
        {
            target.MetadataRecommendations.Add(
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
                });
        }
    }

    // ============================================================
    // Legacy Selection Synchronisation
    // ============================================================

    private static void SyncLegacySelectionState(
        SearchIssue issue)
    {
        issue.SelectedResultId =
            issue.SelectedResultIds.FirstOrDefault();

        issue.SelectionWasRecommended =
            issue.SelectedResultIds.Count == 1 &&
            issue.RecommendedSelectedResultIds.Count == 1 &&
            string.Equals(
                issue.SelectedResultIds[0],
                issue.RecommendedSelectedResultIds[0],
                StringComparison.OrdinalIgnoreCase);
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
                x => x.Name.Equals(
                    run.Category,
                    StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        SelectedCategory =
            category;

        IsSearching =
            App.Services.Search.IsSearching;

        UpdateSearchRunStatus();
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
                category.IsSearchEnabled = true;

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
                x => x.Name.Equals(
                    run.Category,
                    StringComparison.OrdinalIgnoreCase));

        if (category is not null &&
            SelectedCategory is null)
        {
            SelectedCategory =
                category;
        }

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
            IsSearching = false;

            SearchStatus =
                $"{run.IssuesSearched:N0} " +
                $"{run.Category.ToLowerInvariant()} searched • " +
                $"{run.IssuesWithResults:N0} with results";

            UpdateCategorySearchAvailability();
        }
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

        IsSearching = true;

        SearchStatus =
            "Searching...";

        try
        {
            var results =
                await App.Services.Search.SearchAsync(
                    issue,
                    CancellationToken.None);

            issue.Results.Clear();

            issue.SelectedResultIds.Clear();

            issue.RecommendedSelectedResultIds.Clear();

            issue.SelectedResultId =
                string.Empty;

            issue.SelectionWasRecommended =
                false;

            foreach (var result in results)
            {
                result.IsSelected =
                    false;

                issue.Results.Add(
                    result);
            }

            issue.IsSearched =
                true;

            issue.HasResults =
                issue.Results.Count > 0;

            // ----------------------------------------------------
            // Metadata recommendations are produced by the
            // Search service and stored on SearchIssue.
            //
            // Do not convert them into SearchResult objects.
            // ----------------------------------------------------

            NotifyMetadataRecommendationProperties();

            SaveSearchState();

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
            IsSearching = false;

            UpdateCategorySearchAvailability();

            NotifyMetadataRecommendationProperties();
        }
    }

    // ============================================================
    // Metadata Recommendation Selection
    // ============================================================

    /// <summary>
    /// Toggles a single metadata change recommendation.
    ///
    /// This only records the user's selection. It does not modify
    /// the physical file or DIASISS library.
    /// </summary>
    [RelayCommand]
    private void ToggleMetadataRecommendation(
        MetadataChangeRecommendation? recommendation)
    {
        if (recommendation is null)
            return;

        if (!recommendation.IsRecommended)
            return;

        if (!recommendation.IsChange)
            return;

        recommendation.IsSelected =
            !recommendation.IsSelected;

        SaveSearchState();

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Confirm Recommended Metadata Changes
    // ============================================================

    /// <summary>
    /// Selects all recommended metadata changes that meet the
    /// configured confidence threshold.
    ///
    /// This does NOT modify the library.
    ///
    /// It prepares the changes for the later Improve / Recovery
    /// workflow where the user will explicitly confirm and apply
    /// them.
    /// </summary>
    [RelayCommand]
    private void ConfirmRecommendedMetadataChanges()
    {
        var issue =
            SelectedIssue;

        if (issue is null)
            return;

        if (!IsMetadataIssue)
            return;

        foreach (var recommendation in
                 issue.MetadataRecommendations)
        {
            if (!recommendation.IsRecommended)
                continue;

            if (!recommendation.IsChange)
                continue;

            if (recommendation.AgreementPercentage <
                MetadataRecommendationThreshold)
            {
                continue;
            }

            recommendation.IsSelected =
                true;
        }

        SaveSearchState();

        NotifyMetadataRecommendationProperties();
    }

    // ============================================================
    // Duplicate Result Selection
    // ============================================================

    public void SelectResult(
        SearchResult? result)
    {
        if (result is null)
            return;

        var issue =
            SelectedIssue;

        if (issue is null)
            return;

        var matchingResult =
            issue.Results.FirstOrDefault(
                x => string.Equals(
                    x.Id,
                    result.Id,
                    StringComparison.OrdinalIgnoreCase));

        if (matchingResult is null)
            return;

        var alreadySelected =
            matchingResult.IsSelected;

        if (alreadySelected)
        {
            matchingResult.IsSelected =
                false;

            RemoveId(
                issue.SelectedResultIds,
                matchingResult.Id);

            RemoveId(
                issue.RecommendedSelectedResultIds,
                matchingResult.Id);
        }
        else
        {
            matchingResult.IsSelected =
                true;

            AddId(
                issue.SelectedResultIds,
                matchingResult.Id);

            RemoveId(
                issue.RecommendedSelectedResultIds,
                matchingResult.Id);
        }

        SyncLegacySelectionState(
            issue);

        SaveSearchState();

        OnPropertyChanged(
            nameof(AllRecommendedSelected));
    }

    // ============================================================
    // Selection Helpers
    // ============================================================

    private static void AddId(
        ObservableCollection<string> ids,
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (ids.Any(
                x => string.Equals(
                    x,
                    id,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        ids.Add(id);
    }

    private static void RemoveId(
        ObservableCollection<string> ids,
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
                ids.RemoveAt(index);
            }
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

        if (!string.Equals(
                SelectedCategory.Name,
                "Duplicates",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var hasBulkSelections =
            Issues.Any(
                issue =>
                    string.Equals(
                        issue.Category,
                        "Duplicates",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    issue.RecommendedSelectedResultIds.Count > 0);

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
                            x => string.Equals(
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

                SyncLegacySelectionState(
                    issue);

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
                    result => result.IsRecommended);

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

            SyncLegacySelectionState(
                issue);

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
    // All Recommended State
    // ============================================================

    public bool AllRecommendedSelected
    {
        get
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
    }

    public bool IsDuplicateCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase);

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

        if (!category.IsSearchEnabled)
            return;

        var analysisCategoryName =
            GetAnalysisCategoryName(
                category.Name);

        if (analysisCategoryName is null)
            return;

        var categoryIssues =
            Issues
                .Where(x =>
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

        try
        {
            var run =
                await App.Services.Search.SearchAllAsync(
                    categoryIssues,
                    category.Name,
                    CancellationToken.None);

            IsSearching =
                false;

            SearchStatus =
                $"{run.IssuesSearched:N0} " +
                $"{category.Name.ToLowerInvariant()} searched • " +
                $"{run.IssuesWithResults:N0} with results";

            NotifyMetadataRecommendationProperties();
        }
        catch
        {
            IsSearching =
                App.Services.Search.IsSearching;

            throw;
        }
        finally
        {
            UpdateCategorySearchAvailability();

            NotifyMetadataRecommendationProperties();
        }
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
            existing?
                .Issues
                .ToDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<
                string,
                SearchIssue>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var issue in Issues)
        {
            SyncLegacySelectionState(
                issue);

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

                Run =
                    existing?.Run,

                Issues =
                    persistedIssues.Values.ToList()
            };

        App.Services
            .SearchRepository
            .Save(state);
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
    // Analysis Completed
    // ============================================================

    private void AnalysisCompleted(
        object? sender,
        EventArgs e)
    {
        LoadAnalysisSummary();

        RestoreActiveSearchRun();

        UpdateCategorySearchAvailability();

        NotifyMetadataRecommendationProperties();
    }

    private static int GetIssueCount(
        LibraryAnalysisResult analysis,
        string categoryName)
    {
        var category =
            analysis.Categories.FirstOrDefault(
                x => x.Name.Equals(
                    categoryName,
                    StringComparison.OrdinalIgnoreCase));

        return category?.IssueCount ?? 0;
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
        // Improve workspace will be connected here.
    }
}