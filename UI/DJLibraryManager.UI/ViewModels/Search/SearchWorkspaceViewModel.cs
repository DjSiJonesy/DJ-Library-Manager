using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Search;
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

    [ObservableProperty]
    private SearchIssue? selectedIssue;

    // ============================================================
    // Search State
    // ============================================================

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private string searchStatus = "Ready";

    // ============================================================
    // Constructor
    // ============================================================

    public SearchWorkspaceViewModel()
    {
        CreateCategories();

        LoadAnalysisSummary();

        App.Services.ApplicationState.AnalysisCompleted +=
            AnalysisCompleted;
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

            SelectedIssue = null;

            SearchStatus =
                "No analysis available";

            return;
        }

        Summary = new SearchSummary
        {
            AnalysisDate = analysis.AnalysisDate,

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
        SelectIssuesForCategory();
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

        // If the user clicks the already-selected category,
        // refresh the issue list anyway.
        if (ReferenceEquals(
                SelectedCategory,
                category))
        {
            SelectIssuesForCategory();
            return;
        }

        SelectedCategory = category;
    }

    // ============================================================
    // Select Issues For Category
    // ============================================================

    private void SelectIssuesForCategory()
    {
        Issues.Clear();

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

        // ============================================================
        // Build a dictionary of saved Search issues once.
        //
        // This avoids repeatedly scanning the entire saved Search
        // collection for every Analysis issue.
        // ============================================================

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

        // ============================================================
        // Build the current category
        // ============================================================

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

        // ============================================================
        // Select first issue
        // ============================================================

        SelectedIssue =
            Issues.FirstOrDefault();

        SearchStatus =
            Issues.Count == 0
                ? "No issues found"
                : GetCategorySearchStatus();
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
            if (!string.IsNullOrWhiteSpace(path))
            {
                target.RelatedFilePaths.Add(path);
            }
        }

        target.Results.Clear();

        foreach (var result in
                 saved.Results)
        {
            target.Results.Add(result);
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
            ?? new Dictionary<
                string,
                SearchIssue>(
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
    // Search
    // ============================================================

    [RelayCommand]
    private async Task SearchIssue(
        SearchIssue? issue)
    {
        if (issue is null)
            return;

        if (IsSearching)
            return;

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

            foreach (var result in results)
            {
                issue.Results.Add(
                    result);
            }

            issue.IsSearched = true;

            issue.HasResults =
                issue.Results.Count > 0;

            SaveSearchState();

            SearchStatus =
                issue.HasResults
                    ? $"{issue.Results.Count:N0} result(s) found"
                    : "No results found";
        }
        catch
        {
            issue.IsSearched = true;

            issue.HasResults = false;

            SaveSearchState();

            SearchStatus =
                "Search failed";

            throw;
        }
        finally
        {
            IsSearching = false;
        }
    }

    // ============================================================
    // Search All
    // ============================================================

    [RelayCommand]
    private async Task SearchAll(
        SearchCategoryInfo? category)
    {
        if (IsSearching)
            return;

        if (category is null)
            return;

        var categoryIssues =
            Issues
                .Where(x =>
                    x.Category.Equals(
                        GetAnalysisCategoryName(category.Name),
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (categoryIssues.Count == 0)
        {
            SearchStatus =
                $"No {category.Name.ToLowerInvariant()} issues found";

            return;
        }

        IsSearching = true;

        var searched = 0;
        var resultsFound = 0;

        SearchStatus =
            $"Searching 0 of {categoryIssues.Count:N0} " +
            $"{category.Name.ToLowerInvariant()}...";

        try
        {
            foreach (var issue in categoryIssues)
            {
                CancellationToken.None
                    .ThrowIfCancellationRequested();

                var results =
                    await App.Services.Search.SearchAsync(
                        issue,
                        CancellationToken.None);

                issue.Results.Clear();

                foreach (var result in results)
                {
                    issue.Results.Add(result);
                }

                issue.IsSearched = true;

                issue.HasResults =
                    issue.Results.Count > 0;

                searched++;

                if (issue.HasResults)
                {
                    resultsFound +=
                        issue.Results.Count;
                }

                SearchStatus =
                    $"Searching {searched:N0} of " +
                    $"{categoryIssues.Count:N0} " +
                    $"{category.Name.ToLowerInvariant()}...";
            }

            // Save after the entire category has completed.
            SaveSearchState();

            SearchStatus =
                $"{searched:N0} {category.Name.ToLowerInvariant()} searched • " +
                $"{resultsFound:N0} candidates found";
        }
        catch
        {
            // Preserve any searches that completed before
            // the failure occurred.
            SaveSearchState();

            SearchStatus =
                $"Search stopped after {searched:N0} of " +
                $"{categoryIssues.Count:N0} " +
                $"{category.Name.ToLowerInvariant()}";

            throw;
        }
        finally
        {
            IsSearching = false;
        }
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