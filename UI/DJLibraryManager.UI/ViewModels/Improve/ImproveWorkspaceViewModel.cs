using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Services;
using DJLibraryManager.Core.Workflow;

using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Improve;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.ViewModels.Workspace;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels.Improve;

/// <summary>
/// ViewModel for the Improve workspace.
///
/// Improve applies decisions already made by the user during Search.
/// It does not perform new searches or generate new recommendations.
/// </summary>
public partial class ImproveWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title =>
        "Improve";

    public override string Subtitle =>
        "Apply the changes selected during Search to the DIASISS library.";

    // ============================================================
    // Categories
    // ============================================================

    public ObservableCollection<ImproveCategoryInfo> Categories { get; }
        = new();

    // ============================================================
    // Selected Category
    // ============================================================

    [ObservableProperty]
    private ImproveCategoryInfo? selectedCategory;

    // ============================================================
    // Duplicates Summary
    // ============================================================

    [ObservableProperty]
    private int totalDuplicates;

    [ObservableProperty]
    private int duplicatesToKeep;

    [ObservableProperty]
    private int duplicatesToMove;

    // ============================================================
    // Workspace State
    // ============================================================

    [ObservableProperty]
    private string status = "Ready";

    // ============================================================
    // Category Visibility
    // ============================================================

    public bool IsDuplicatesCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase);

    public bool IsMissingFilesCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Missing Files",
            StringComparison.OrdinalIgnoreCase);

    public bool IsMetadataCategory =>
        string.Equals(
            SelectedCategory?.Name,
            "Metadata",
            StringComparison.OrdinalIgnoreCase);

    // ============================================================
    // Constructor
    // ============================================================

    public ImproveWorkspaceViewModel()
    {
        CreateCategories();

        SelectedCategory =
            Categories.FirstOrDefault();

        LoadCurrentSearchState();
    }

    // ============================================================
    // Create Categories
    // ============================================================

    private void CreateCategories()
    {
        Categories.Clear();

        Categories.Add(
            new ImproveCategoryInfo
            {
                Name = "Duplicates",
                Icon = "📑",
                Description =
                    "Apply the duplicate selections made during Search, Unselected copies will be moved to the DIASISS Duplicates folder."
            });

        Categories.Add(
            new ImproveCategoryInfo
            {
                Name = "Missing Files",
                Icon = "⚠️",
                Description =
                    "Remove missing file records from the relevant Provider data."
            });

        Categories.Add(
            new ImproveCategoryInfo
            {
                Name = "Metadata",
                Icon = "📋",
                Description =
                    "Apply selected metadata changes from Search or Import an edited metadata export file."
            });
    }

    // ============================================================
    // Selected Category Changed
    // ============================================================

    partial void OnSelectedCategoryChanged(
        ImproveCategoryInfo? value)
    {
        OnPropertyChanged(
            nameof(IsDuplicatesCategory));

        OnPropertyChanged(
            nameof(IsMissingFilesCategory));

        OnPropertyChanged(
            nameof(IsMetadataCategory));

        LoadCurrentSearchState();
    }

    // ============================================================
    // Load Search State
    // ============================================================

    /// <summary>
    /// Reads the existing Search state.
    ///
    /// Improve deliberately does not perform another search.
    /// </summary>
    private void LoadCurrentSearchState()
    {
        ResetSummary();

        var search =
            App.Services
                .SearchRepository
                .CurrentSearch;

        if (search is null)
        {
            Status =
                "No Search results available";

            return;
        }

        UpdateCategoryCounts(search);

        // --------------------------------------------------------
        // Duplicates
        // --------------------------------------------------------

        if (IsDuplicatesCategory)
        {
            LoadDuplicateSummary(search);

            return;
        }

        // --------------------------------------------------------
        // Missing Files
        // --------------------------------------------------------

        if (IsMissingFilesCategory)
        {
            LoadMissingFilesSummary(search);

            return;
        }

        // --------------------------------------------------------
        // Metadata
        // --------------------------------------------------------

        if (IsMetadataCategory)
        {
            LoadMetadataSummary(search);

            return;
        }
    }

    // ============================================================
    // Category Counts
    // ============================================================

    /// <summary>
    /// Updates the counts displayed on the Improve category
    /// buttons using the existing Search results.
    ///
    /// Improve does not perform a new search.
    ///
    /// Search uses "File Integrity" as the underlying Analysis
    /// category for the user-facing "Missing Files" category.
    /// </summary>
    private void UpdateCategoryCounts(
        SearchState search)
    {
        foreach (var category in Categories)
        {
            var searchCategoryName =
                GetSearchCategoryName(
                    category.Name);

            category.Count =
                search.Issues.Count(
                    issue =>
                        string.Equals(
                            issue.Category,
                            searchCategoryName,
                            StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Maps the user-facing Improve category name to the
    /// underlying Search issue category name.
    /// </summary>
    private static string GetSearchCategoryName(
        string categoryName)
    {
        return categoryName switch
        {
            "Duplicates" =>
                "Duplicates",

            "Missing Files" =>
                "File Integrity",

            "Metadata" =>
                "Metadata",

            _ =>
                categoryName
        };
    }

    // ============================================================
    // Duplicate Summary
    // ============================================================

    /// <summary>
    /// Builds the Duplicate Improve summary.
    ///
    /// A Duplicate issue represents a group of files containing
    /// duplicate/multiple copies. Each issue can therefore contain
    /// multiple individual files in its Results collection.
    ///
    /// The summary distinguishes between:
    ///
    /// - Duplicate/multiple-copy groups
    /// - Total individual files in those groups
    /// - Individual files selected to keep
    /// - Individual files that will be moved to DIASISS Duplicates
    /// </summary>
    private void LoadDuplicateSummary(
        SearchState search)
    {
        var duplicateIssues =
            search.Issues
                .Where(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Duplicates",
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        // ------------------------------------------------------------
        // Total individual duplicate files
        // ------------------------------------------------------------

        TotalDuplicates =
            duplicateIssues.Sum(
                issue =>
                    issue.Results.Count);

        // ------------------------------------------------------------
        // Individual files selected to keep
        // ------------------------------------------------------------

        DuplicatesToKeep =
            duplicateIssues.Sum(
                issue =>
                    issue.SelectedResultIds.Count);

        // ------------------------------------------------------------
        // Individual files that will be moved
        // ------------------------------------------------------------

        DuplicatesToMove =
            Math.Max(
                TotalDuplicates - DuplicatesToKeep,
                0);

        // ------------------------------------------------------------
        // No duplicate files
        // ------------------------------------------------------------

        if (duplicateIssues.Count == 0)
        {
            Status =
                "0 Files with duplicate/multiple copies:\n" +
                "Totalling 0 files.\n" +
                "0 individual copies selected to keep.\n" +
                "0 files will be moved to DIASISS Duplicates.";

            return;
        }

        // ------------------------------------------------------------
        // Duplicate summary
        // ------------------------------------------------------------

        Status =
            $"{duplicateIssues.Count:N0} Files with duplicate/multiple copies.\n" +
            $"{TotalDuplicates:N0} Total files.\n" +
            $"{DuplicatesToKeep:N0} individual copies selected to keep.\n" +
            $"{DuplicatesToMove:N0} files will be moved to DIASISS Duplicates.";
    }

    // ============================================================
    // Missing Files Summary
    // ============================================================

    /// <summary>
    /// Builds the Missing Files Improve summary.
    ///
    /// Missing Files are represented by the File Integrity
    /// category in Analysis/Search.
    /// </summary>
    private void LoadMissingFilesSummary(
        SearchState search)
    {
        var missingFileCount =
            search.Issues.Count(
                issue =>
                    string.Equals(
                        issue.Category,
                        "File Integrity",
                        StringComparison.OrdinalIgnoreCase));

        Status =
            $"{missingFileCount:N0} Missing files ready to remove.";
    }

    // ============================================================
    // Metadata Summary
    // ============================================================

    /// <summary>
    /// Builds the Metadata Improve summary.
    ///
    /// The first number is the original number of Metadata
    /// issues found during Analysis/Search.
    ///
    /// The second number is the number of Metadata issues for
    /// which Search found one or more results.
    ///
    /// This deliberately counts issues/tracks rather than
    /// individual metadata field recommendations.
    /// </summary>
    private void LoadMetadataSummary(
        SearchState search)
    {
        var metadataIssues =
            search.Issues
                .Where(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Metadata",
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        var improvementsFound =
            metadataIssues.Count(
                issue =>
                    issue.HasResults);

        Status =
            $"{metadataIssues.Count:N0} Metadata issues originally found. Improvements found to " +
            $"{improvementsFound:N0} Tracks.";
    }

    // ============================================================
    // Reset Summary
    // ============================================================

    private void ResetSummary()
    {
        TotalDuplicates = 0;
        DuplicatesToKeep = 0;
        DuplicatesToMove = 0;

        foreach (var category in Categories)
        {
            category.Count = 0;
        }

        Status = "Ready";
    }

    // ============================================================
    // Category Selection
    // ============================================================

    [RelayCommand]
    private void SelectCategory(
        ImproveCategoryInfo? category)
    {
        if (category is null)
            return;

        SelectedCategory =
            category;
    }

    // ============================================================
    // Open DIASISS Duplicates Folder
    // ============================================================

    /// <summary>
    /// Opens the DIASISS Duplicates folder.
    ///
    /// The folder is created if it does not already exist.
    /// </summary>
    [RelayCommand]
    private void OpenDuplicates()
    {
        var path =
            ApplicationPaths.DiasissDuplicates;

        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            Directory.CreateDirectory(path);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Unable to open DIASISS Duplicates folder: {exception}");
        }
    }

    // ============================================================
    // Import Metadata
    // ============================================================

    /// <summary>
    /// Placeholder for importing an edited metadata export.
    ///
    /// The actual file-picker and metadata import pipeline will be
    /// implemented when the Metadata Improve functionality is built.
    /// </summary>
    [RelayCommand]
    private void ImportMetadata()
    {
        Status =
            "Metadata import is not yet implemented";
    }

    // ============================================================
    // Confirm
    // ============================================================

    /// <summary>
    /// Confirm the currently displayed Improve operation.
    ///
    /// Actual library modification will be implemented separately.
    /// </summary>
    [RelayCommand]
    private void Confirm()
    {
        Status =
            "Improve operation is not yet implemented";
    }

    // ============================================================
    // Go Back
    // ============================================================

    /// <summary>
    /// Returns to the Search workspace.
    /// </summary>
    [RelayCommand]
    private void Previous()
    {
        App.Services
            .ApplicationState
            .NavigateTo(
                WorkspaceType.Search);
    }

    // ============================================================
    // Next
    // ============================================================

    /// <summary>
    /// Moves to the Structure workflow.
    /// </summary>
    [RelayCommand]
    private void Next()
    {
        App.Services
            .ApplicationState
            .NavigateTo(
                WorkspaceType.Structure);
    }
}