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
using System.Threading.Tasks;

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
    // Operation Result
    // ============================================================

    /// <summary>
    /// Contains the result of the most recently completed Improve
    /// operation.
    ///
    /// This is deliberately separate from Status because Status is
    /// also used for the pre-operation Search/Improve summary.
    /// </summary>
    [ObservableProperty]
    private string operationResult = "No operation has been run.";

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
    ///
    /// Category counts come from the latest completed Analysis,
    /// matching the Search Workspace.
    ///
    /// The SearchState remains the source of the user's existing
    /// Improve decisions and selections.
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

        // Category counts reflect the latest completed Analysis,
        // just as they do in the Search Workspace.
        UpdateCategoryCounts();

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
    /// buttons using the latest completed Analysis.
    ///
    /// This deliberately matches the Search Workspace, where the
    /// category counts are based on AnalysisRepository.CurrentAnalysis.
    ///
    /// SearchState is still used separately for the user's Search
    /// decisions and selections.
    /// </summary>
    private void UpdateCategoryCounts()
    {
        var analysis =
            App.Services
                .AnalysisRepository
                .CurrentAnalysis;

        if (analysis is null)
        {
            foreach (var category in Categories)
            {
                category.Count = 0;
            }

            return;
        }

        foreach (var category in Categories)
        {
            var analysisCategoryName =
                GetAnalysisCategoryName(
                    category.Name);

            var analysisCategory =
                analysis.Categories.FirstOrDefault(
                    result =>
                        string.Equals(
                            result.Name,
                            analysisCategoryName,
                            StringComparison.OrdinalIgnoreCase));

            category.Count =
                analysisCategory?.IssueCount ?? 0;
        }
    }

    /// <summary>
    /// Maps the user-facing Improve category name to the
    /// underlying Analysis category name.
    /// </summary>
    private static string GetAnalysisCategoryName(
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
    /// Confirms and applies the currently selected Improve operation.
    ///
    /// For Duplicates, every SearchResult that has NOT been selected
    /// to keep is physically moved to the DIASISS Duplicates folder.
    ///
    /// Selected files are never moved.
    ///
    /// Every successful physical move is recorded in FileChanges
    /// using the authoritative DIASISS MediaId and the original and
    /// new physical paths.
    /// </summary>
    [RelayCommand]
    private async Task Confirm()
    {
        // --------------------------------------------------------
        // Only Duplicates is implemented at this stage.
        // --------------------------------------------------------

        if (!IsDuplicatesCategory)
        {
            Status =
                "Improve operation is not yet implemented";

            return;
        }

        var search =
            App.Services
                .SearchRepository
                .CurrentSearch;

        if (search is null)
        {
            Status =
                "No Search results available";

            OperationResult =
                "No operation has been run.";

            return;
        }

        var destination =
            ApplicationPaths.DiasissDuplicates;

        if (string.IsNullOrWhiteSpace(destination))
        {
            Status =
                "DIASISS Duplicates folder is not configured";

            OperationResult =
                "No operation has been run.";

            return;
        }

        var duplicateIssues =
            search.Issues
                .Where(
                    issue =>
                        string.Equals(
                            issue.Category,
                            "Duplicates",
                            StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (duplicateIssues.Count == 0)
        {
            Status =
                "No duplicate files are available to apply.";

            OperationResult =
                "No operation has been run.";

            return;
        }

        try
        {
            Directory.CreateDirectory(destination);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Unable to create DIASISS Duplicates folder: {exception}");

            Status =
                $"Unable to create DIASISS Duplicates folder: {exception.Message}";

            OperationResult =
                "No operation was completed.";

            return;
        }

        // --------------------------------------------------------
        // One OperationId identifies this entire Confirm operation.
        // Every FileChanges record created below shares this ID.
        // --------------------------------------------------------

        var operationId =
            Guid.NewGuid().ToString("N");

        var movedCount = 0;
        var missingCount = 0;
        var failedCount = 0;
        var skippedCount = 0;

        // --------------------------------------------------------
        // This is the number of files that the user asked Improve
        // to move before the operation actually starts.
        //
        // It is deliberately captured from the existing Improve
        // selection summary and is not used as the success count.
        // --------------------------------------------------------

        var selectedForMove =
            duplicateIssues.Sum(
                issue =>
                    issue.Results.Count -
                    issue.SelectedResultIds.Count);

        foreach (var issue in duplicateIssues)
        {
            // ----------------------------------------------------
            // SelectedResultIds contains the files the user chose
            // to KEEP. Everything else is eligible to be moved.
            // ----------------------------------------------------

            var selectedResultIds =
                issue.SelectedResultIds
                    .ToHashSet(
                        StringComparer.OrdinalIgnoreCase);

            foreach (var result in issue.Results)
            {
                if (selectedResultIds.Contains(result.Id))
                {
                    continue;
                }

                var sourcePath =
                    result.FilePath;

                // ------------------------------------------------
                // No source path means the physical file cannot be
                // located. Treat this as a missing file.
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    missingCount++;

                    Debug.WriteLine(
                        $"Duplicate result {result.Id} has no file path.");

                    continue;
                }

                // ------------------------------------------------
                // A physical move must never occur unless the
                // result has a valid DIASISS MediaId that can be
                // recorded for recovery.
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(result.MediaId))
                {
                    failedCount++;

                    Debug.WriteLine(
                        $"Duplicate result {result.Id} has no DIASISS MediaId. File was not moved.");

                    continue;
                }

                try
                {
                    if (!File.Exists(sourcePath))
                    {
                        missingCount++;

                        Debug.WriteLine(
                            $"Duplicate source file no longer exists: {sourcePath}");

                        continue;
                    }

                    var destinationPath =
                        GetUniqueDestinationPath(
                            destination,
                            Path.GetFileName(sourcePath));

                    // ------------------------------------------------
                    // If the file is already in the destination folder
                    // at the same path, there is nothing to do.
                    // ------------------------------------------------

                    if (PathsEqual(
                        sourcePath,
                        destinationPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    // ------------------------------------------------
                    // Physical move.
                    // ------------------------------------------------

                    MoveFile(
                        sourcePath,
                        destinationPath);

                    // ------------------------------------------------
                    // The physical move succeeded.
                    //
                    // Record the exact before/after paths together
                    // with the authoritative DIASISS MediaId.
                    // ------------------------------------------------

                    try
                    {
                        await App.Services
                            .FileChangeRepository
                            .RecordChangeAsync(
                                operationId,
                                "Improve",
                                "Duplicate",
                                result.MediaId,
                                sourcePath,
                                destinationPath,
                                "Completed");
                    }
                    catch (Exception recordException)
                    {
                        // ------------------------------------------------
                        // The physical change succeeded but the recovery
                        // record could not be persisted.
                        //
                        // Attempt to restore the physical file immediately
                        // so that we do not leave an unrecorded change.
                        // ------------------------------------------------

                        failedCount++;

                        Debug.WriteLine(
                            $"Unable to record Duplicate FileChange for '{sourcePath}': {recordException}");

                        try
                        {
                            MoveFile(
                                destinationPath,
                                sourcePath);

                            Debug.WriteLine(
                                $"Duplicate file restored because its FileChanges record could not be written: {sourcePath}");
                        }
                        catch (Exception restoreException)
                        {
                            Debug.WriteLine(
                                $"CRITICAL: Duplicate file was moved but could not be restored after FileChanges recording failed. " +
                                $"Original='{sourcePath}', New='{destinationPath}', " +
                                $"MediaId='{result.MediaId}', " +
                                $"RecordError='{recordException}', " +
                                $"RestoreError='{restoreException}'");
                        }

                        continue;
                    }

                    movedCount++;

                    Debug.WriteLine(
                        $"Moved duplicate file: {sourcePath} -> {destinationPath}");

                    Debug.WriteLine(
                        $"Recorded FileChange: OperationId={operationId}, " +
                        $"MediaId={result.MediaId}, " +
                        $"OriginalPath={sourcePath}, " +
                        $"NewPath={destinationPath}, " +
                        $"Status=Completed");
                }
                catch (Exception exception)
                {
                    failedCount++;

                    Debug.WriteLine(
                        $"Unable to move duplicate file '{sourcePath}': {exception}");
                }
            }
        }

        // --------------------------------------------------------
        // Update the Operation Result with what ACTUALLY happened.
        //
        // This is deliberately separate from Status, which contains
        // the pre-operation Improve summary.
        // --------------------------------------------------------

        OperationResult =
            $"{selectedForMove:N0} files selected for move.\n" +
            $"{movedCount:N0} files successfully moved to DIASISS Duplicates.\n" +
            $"{missingCount:N0} files were missing and could not be moved.\n" +
            $"{failedCount:N0} files encountered errors.";

        if (skippedCount > 0)
        {
            OperationResult +=
                $"\n{skippedCount:N0} files were skipped.";
        }

        // --------------------------------------------------------
        // The physical files have changed, but SearchState remains
        // the record of the user's Search decisions. Do not rebuild
        // or overwrite Search results here.
        // --------------------------------------------------------

        Debug.WriteLine(
            $"Duplicate Improve complete. OperationId={operationId}, " +
            $"SelectedForMove={selectedForMove}, " +
            $"Moved={movedCount}, " +
            $"Missing={missingCount}, " +
            $"Failed={failedCount}, " +
            $"Skipped={skippedCount}.");

        // --------------------------------------------------------
        // Automatic post-operation Analysis refresh.
        //
        // The library has now physically changed, so run Analysis
        // against the current library state.
        //
        // This deliberately does NOT replace SearchState.
        // SearchState remains the user's original Search decisions.
        // --------------------------------------------------------

        await RefreshAnalysisAfterDuplicateOperationAsync();
    }

    // ============================================================
    // Refresh Analysis After Duplicate Operation
    // ============================================================

    /// <summary>
    /// Re-analyses the current library after a Duplicate Improve
    /// operation and refreshes the Improve category counts.
    ///
    /// SearchState is deliberately preserved because it contains
    /// the user's original Search decisions.
    /// </summary>
    private async Task RefreshAnalysisAfterDuplicateOperationAsync()
    {
        try
        {
            Debug.WriteLine(
                "Starting automatic Analysis refresh after Duplicate Improve operation.");

            var analysis =
                await App.Services
                    .Analysis
                    .AnalyseLibraryAsync();

            // --------------------------------------------------------
            // Persist the newly completed analysis so the rest of the
            // application has the current library state available.
            // --------------------------------------------------------

            App.Services
                .AnalysisRepository
                .Save(analysis);

            // --------------------------------------------------------
            // Update the Improve category counts from the newly saved
            // Analysis.
            //
            // This is the same source used when Improve is first
            // opened, and matches the Search Workspace.
            // --------------------------------------------------------

            UpdateCategoryCounts();

            // --------------------------------------------------------
            // Refresh the current category's displayed state where
            // the fresh Analysis result can provide an authoritative
            // current count.
            // --------------------------------------------------------

            var duplicatesAnalysis =
                analysis.Categories.FirstOrDefault(
                    category =>
                        string.Equals(
                            category.Name,
                            "Duplicates",
                            StringComparison.OrdinalIgnoreCase));

            var missingFilesAnalysis =
                analysis.Categories.FirstOrDefault(
                    category =>
                        string.Equals(
                            category.Name,
                            "File Integrity",
                            StringComparison.OrdinalIgnoreCase));

            if (IsDuplicatesCategory &&
                duplicatesAnalysis is not null &&
                duplicatesAnalysis.IssueCount == 0)
            {
                TotalDuplicates = 0;
                DuplicatesToKeep = 0;
                DuplicatesToMove = 0;

                Status =
                    "0 Files with duplicate/multiple copies:\n" +
                    "Totalling 0 files.\n" +
                    "0 individual copies selected to keep.\n" +
                    "0 files will be moved to DIASISS Duplicates.";
            }

            if (IsMissingFilesCategory &&
                missingFilesAnalysis is not null)
            {
                Status =
                    $"{missingFilesAnalysis.IssueCount:N0} Missing files ready to remove.";
            }

            Debug.WriteLine(
                $"Automatic Analysis refresh complete. " +
                $"Duplicates={duplicatesAnalysis?.IssueCount ?? 0}, " +
                $"MissingFiles={missingFilesAnalysis?.IssueCount ?? 0}.");
        }
        catch (Exception exception)
        {
            // --------------------------------------------------------
            // The Duplicate operation itself has already completed.
            //
            // A failure here must not undo the completed FileChanges
            // records or physical moves. Report the refresh failure
            // separately so the user knows the operation succeeded but
            // the automatic Analysis refresh did not.
            // --------------------------------------------------------

            Debug.WriteLine(
                $"Automatic Analysis refresh after Duplicate Improve failed: {exception}");

            OperationResult +=
                "\nAutomatic Analysis refresh failed. Please run Analyse Library manually.";
        }
    }

    // ============================================================
    // Get Unique Destination Path
    // ============================================================

    /// <summary>
    /// Creates a destination path that will not overwrite an
    /// existing file.
    ///
    /// Example:
    ///
    /// Track.mp3
    /// Track (1).mp3
    /// Track (2).mp3
    /// </summary>
    private static string GetUniqueDestinationPath(
        string destinationDirectory,
        string fileName)
    {
        var destinationPath =
            Path.Combine(
                destinationDirectory,
                fileName);

        if (!File.Exists(destinationPath))
            return destinationPath;

        var baseName =
            Path.GetFileNameWithoutExtension(fileName);

        var extension =
            Path.GetExtension(fileName);

        var counter = 1;

        while (true)
        {
            var candidate =
                Path.Combine(
                    destinationDirectory,
                    $"{baseName} ({counter}){extension}");

            if (!File.Exists(candidate))
                return candidate;

            counter++;
        }
    }

    // ============================================================
    // Move File
    // ============================================================

    /// <summary>
    /// Moves a file to the destination.
    ///
    /// File.Move is used first so that files on the same volume
    /// are moved without an unnecessary copy.
    ///
    /// If the move fails because the source and destination are
    /// on different volumes, the operation falls back to:
    ///
    /// Copy -> verify destination -> Delete source
    ///
    /// This allows the DIASISS Duplicates folder to reside on a
    /// different drive from the original music file.
    /// </summary>
    private static void MoveFile(
        string sourcePath,
        string destinationPath)
    {
        try
        {
            File.Move(
                sourcePath,
                destinationPath);

            return;
        }
        catch (IOException)
        {
            // ----------------------------------------------------
            // File.Move can fail when source and destination are
            // located on different volumes. Fall through to the
            // copy/delete implementation.
            // ----------------------------------------------------
        }

        File.Copy(
            sourcePath,
            destinationPath,
            overwrite: false);

        // --------------------------------------------------------
        // Verify the destination exists before deleting the source.
        // --------------------------------------------------------

        if (!File.Exists(destinationPath))
        {
            throw new IOException(
                "The duplicate file was copied but the destination " +
                "file could not be verified.");
        }

        File.Delete(sourcePath);
    }

    // ============================================================
    // Compare Paths
    // ============================================================

    /// <summary>
    /// Determines whether two file paths refer to the same physical
    /// path.
    /// </summary>
    private static bool PathsEqual(
        string firstPath,
        string secondPath)
    {
        try
        {
            var firstFullPath =
                Path.GetFullPath(firstPath)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            var secondFullPath =
                Path.GetFullPath(secondPath)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            return string.Equals(
                firstFullPath,
                secondFullPath,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(
                firstPath,
                secondPath,
                StringComparison.OrdinalIgnoreCase);
        }
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