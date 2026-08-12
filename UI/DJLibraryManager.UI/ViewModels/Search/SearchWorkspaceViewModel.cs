using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.ViewModels.Workspace;
using System;
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
                Description =
                    "Find duplicate tracks and compare the available copies."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Missing Files",
                Description =
                    "Investigate files that are no longer available at their recorded location."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Metadata",
                Description =
                    "Find missing or incomplete track metadata."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Music",
                Description =
                    "Investigate BPM, Key and Duration information."
            });

        Categories.Add(
            new SearchCategoryInfo
            {
                Name = "Providers",
                Description =
                    "Investigate provider associations for library tracks."
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
                    "Metadata"),

            MusicIssueCount =
                GetIssueCount(
                    analysis,
                    "Music"),

            ProviderIssueCount =
                GetIssueCount(
                    analysis,
                    "Providers")
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

        SetCategoryCount(
            "Music",
            Summary.MusicIssueCount);

        SetCategoryCount(
            "Providers",
            Summary.ProviderIssueCount);
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

        foreach (var issue in category.Issues)
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

            Issues.Add(
                searchIssue);
        }

        SelectedIssue =
            Issues.FirstOrDefault();

        SearchStatus =
            Issues.Count == 0
                ? "No issues found"
                : $"{Issues.Count:N0} issues available";
    }

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

            "Music" =>
                "Music",

            "Providers" =>
                "Providers",

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

            SearchStatus =
                issue.HasResults
                    ? $"{issue.Results.Count:N0} result(s) found"
                    : "No results found";
        }
        catch
        {
            issue.IsSearched = true;

            issue.HasResults = false;

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
    // Search All Duplicates
    // ============================================================

    [RelayCommand]
    private async Task SearchAllDuplicates()
    {
        if (IsSearching)
            return;

        var duplicateIssues =
            Issues
                .Where(x =>
                    x.Category.Equals(
                        "Duplicates",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (duplicateIssues.Count == 0)
        {
            SearchStatus =
                "No duplicate issues found";

            return;
        }

        IsSearching = true;

        var searched = 0;
        var resultsFound = 0;

        SearchStatus =
            $"Searching 0 of {duplicateIssues.Count:N0} duplicates...";

        try
        {
            foreach (var issue in duplicateIssues)
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
                    issue.Results.Add(
                        result);
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
                    $"{duplicateIssues.Count:N0} duplicates...";
            }

            SearchStatus =
                $"{searched:N0} duplicates searched • " +
                $"{resultsFound:N0} candidates found";
        }
        catch
        {
            SearchStatus =
                $"Search stopped after {searched:N0} of " +
                $"{duplicateIssues.Count:N0} duplicates";

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