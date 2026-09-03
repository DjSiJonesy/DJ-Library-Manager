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
using System.Collections.Generic;
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
    public event Func<string, string, string, Task<bool>>? ConfirmationRequested;

    private async Task<bool> RequestConfirmationAsync(
        string title,
        string message,
        string confirmButtonText)
    {
        if (ConfirmationRequested is null)
            return false;

        return await ConfirmationRequested(
            title,
            message,
            confirmButtonText);
    }

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
    // Restore Points
    // ============================================================

    /// <summary>
    /// Restore points created by completed Improve operations.
    ///
    /// The collection is ordered from oldest operation to newest
    /// operation.
    /// </summary>
    public ObservableCollection<RestorePointInfo> RestorePoints { get; }
        = new();

    /// <summary>
    /// The restore point currently selected by the user.
    /// </summary>
    [ObservableProperty]
    private RestorePointInfo? selectedRestorePoint;

    /// <summary>
    /// Indicates whether restore points are currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool isLoadingRestorePoints;

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

        _ = LoadRestorePointsAsync();
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

        UpdateCategoryCounts();

        if (IsDuplicatesCategory)
        {
            LoadDuplicateSummary(search);
            return;
        }

        if (IsMissingFilesCategory)
        {
            LoadMissingFilesSummary(search);
            return;
        }

        if (IsMetadataCategory)
        {
            LoadMetadataSummary(search);
            return;
        }
    }

    // ============================================================
    // Category Counts
    // ============================================================

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

        TotalDuplicates =
            duplicateIssues.Sum(
                issue =>
                    issue.Results.Count);

        DuplicatesToKeep =
            duplicateIssues.Sum(
                issue =>
                    issue.SelectedResultIds.Count);

        DuplicatesToMove =
            Math.Max(
                TotalDuplicates - DuplicatesToKeep,
                0);

        if (duplicateIssues.Count == 0)
        {
            Status =
                "0 Files with duplicate/multiple copies:\n" +
                "Totalling 0 files.\n" +
                "0 individual copies selected to keep.\n" +
                "0 files will be moved to DIASISS Duplicates.";

            return;
        }

        Status =
            $"{duplicateIssues.Count:N0} Files with duplicate/multiple copies.\n" +
            $"{TotalDuplicates:N0} Total files.\n" +
            $"{DuplicatesToKeep:N0} individual copies selected to keep.\n" +
            $"{DuplicatesToMove:N0} files will be moved to DIASISS Duplicates.";
    }

    // ============================================================
    // Missing Files Summary
    // ============================================================

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
    // Load Restore Points
    // ============================================================

    private async Task LoadRestorePointsAsync()
    {
        try
        {
            IsLoadingRestorePoints = true;

            var changes =
                await App.Services.FileChangeRepository
                    .GetStageChangesAsync("Improve");

            var completedChanges = changes
                .Where(c =>
                    string.Equals(
                        c.Status,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var operationGroups = completedChanges
                .GroupBy(c => c.OperationId)
                .Select(group =>
                {
                    var orderedChanges = group
                        .OrderBy(c => GetChangedDate(c.ChangedDate))
                        .ThenBy(c => c.ChangeId)
                        .ToList();

                    var firstChange = orderedChanges.First();

                    return new
                    {
                        OperationId = group.Key,
                        ChangedDate =
                            GetChangedDate(firstChange.ChangedDate),
                        FirstChangeId = firstChange.ChangeId,
                        ChangeCount = group.Count()
                    };
                })
                .OrderBy(x => x.ChangedDate)
                .ThenBy(x => x.FirstChangeId)
                .ToList();

            RestorePoints.Clear();

            for (int i = 0; i < operationGroups.Count; i++)
            {
                var operation = operationGroups[i];

                int changesToRestore =
                    operationGroups
                        .Skip(i)
                        .Sum(x => x.ChangeCount);

                int operationsToRestore =
                    operationGroups.Count - i;

                RestorePoints.Add(
                    new RestorePointInfo(
                        operation.OperationId,
                        operation.ChangedDate,
                        operation.ChangeCount,
                        operationsToRestore,
                        changesToRestore));
            }

            SelectedRestorePoint =
                RestorePoints.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"Failed to load restore points: {ex}");

            RestorePoints.Clear();
            SelectedRestorePoint = null;
        }
        finally
        {
            IsLoadingRestorePoints = false;
        }
    }

    // ============================================================
    // Parse Changed Date
    // ============================================================

    private static DateTime GetChangedDate(
        string changedDate)
    {
        if (DateTime.TryParse(
                changedDate,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            return parsed;
        }

        return DateTime.MinValue;
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
    // Restore Point Selection
    // ============================================================

    [RelayCommand]
    private void SelectRestorePoint(
        RestorePointInfo? restorePoint)
    {
        if (restorePoint is null)
            return;

        SelectedRestorePoint =
            restorePoint;
    }

    // ============================================================
    // Restore Selected Changes
    // ============================================================

    /// <summary>
    /// Restores the selected Improve operation and every Improve
    /// operation that occurred after it.
    ///
    /// Earlier operations are deliberately left untouched.
    ///
    /// The restore is recovery-first:
    ///
    /// 1. Load all completed Improve changes.
    /// 2. Determine the selected operation and every later operation.
    /// 3. Preflight every physical source/destination path.
    /// 4. Request user confirmation.
    /// 5. Restore files in reverse operation order.
    /// 6. Mark successfully restored FileChanges as Restored.
    /// 7. Refresh Analysis.
    ///
    /// No SearchState is modified.
    /// </summary>
    [RelayCommand]
    private async Task RestoreSelectedChanges()
    {
        if (SelectedRestorePoint is null)
        {
            Status =
                "Select a restore point before restoring changes.";

            return;
        }

        try
        {
            Status =
                "Preparing restore...";

            var changes =
                await App.Services
                    .FileChangeRepository
                    .GetStageChangesAsync("Improve");

            var completedChanges =
                changes
                    .Where(change =>
                        string.Equals(
                            change.Status,
                            "Completed",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (completedChanges.Count == 0)
            {
                Status =
                    "There are no completed Improve changes available to restore.";

                return;
            }

            var operationGroups =
                completedChanges
                    .GroupBy(change => change.OperationId)
                    .Select(group =>
                    {
                        var orderedChanges =
                            group
                                .OrderBy(change =>
                                    GetChangedDate(change.ChangedDate))
                                .ThenBy(change => change.ChangeId)
                                .ToList();

                        var firstChange =
                            orderedChanges.First();

                        return new
                        {
                            OperationId = group.Key,
                            ChangedDate =
                                GetChangedDate(
                                    firstChange.ChangedDate),
                            FirstChangeId =
                                firstChange.ChangeId,
                            Changes =
                                orderedChanges
                        };
                    })
                    .OrderBy(operation => operation.ChangedDate)
                    .ThenBy(operation => operation.FirstChangeId)
                    .ToList();

            var selectedIndex =
                operationGroups.FindIndex(
                    operation =>
                        string.Equals(
                            operation.OperationId,
                            SelectedRestorePoint.OperationId,
                            StringComparison.OrdinalIgnoreCase));

            if (selectedIndex < 0)
            {
                Status =
                    "The selected restore point is no longer available.";

                return;
            }

            var operationsToRestore =
                operationGroups
                    .Skip(selectedIndex)
                    .ToList();

            var changesToRestore =
                operationsToRestore
                    .SelectMany(operation => operation.Changes)
                    .ToList();

            if (changesToRestore.Count == 0)
            {
                Status =
                    "There are no completed changes available at the selected restore point.";

                return;
            }

            // --------------------------------------------------------
            // PRE-FLIGHT
            //
            // Nothing is physically changed until every change has
            // passed these checks.
            // --------------------------------------------------------

            var preflightErrors =
                new List<string>();

            foreach (var change in changesToRestore)
            {
                if (string.IsNullOrWhiteSpace(change.MediaId))
                {
                    preflightErrors.Add(
                        $"Change {change.ChangeId}: missing MediaId.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(change.NewPath))
                {
                    preflightErrors.Add(
                        $"Change {change.ChangeId}: missing NewPath.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(change.OriginalPath))
                {
                    preflightErrors.Add(
                        $"Change {change.ChangeId}: missing OriginalPath.");

                    continue;
                }

                if (!File.Exists(change.NewPath))
                {
                    preflightErrors.Add(
                        $"Change {change.ChangeId}: restored file does not exist at '{change.NewPath}'.");
                }

                if (File.Exists(change.OriginalPath))
                {
                    preflightErrors.Add(
                        $"Change {change.ChangeId}: original path already exists at '{change.OriginalPath}'.");
                }
            }

            if (preflightErrors.Count > 0)
            {
                Status =
                    $"Restore cannot proceed. {preflightErrors.Count:N0} pre-flight issue(s) were found.";

                OperationResult =
                    "No files were restored.\n\n" +
                    string.Join(
                        "\n",
                        preflightErrors.Take(10));

                if (preflightErrors.Count > 10)
                {
                    OperationResult +=
                        $"\n...and {preflightErrors.Count - 10:N0} more.";
                }

                return;
            }

            // --------------------------------------------------------
            // User confirmation.
            //
            // All restore validation has passed. No physical files
            // have been changed at this point.
            // --------------------------------------------------------

            var selectedRestoreDate =
                SelectedRestorePoint.DisplayDate;

            var restoreConfirmationMessage =
                $"Restore from {selectedRestoreDate}?\n\n" +
                $"{operationsToRestore.Count:N0} Improve operations and " +
                $"{changesToRestore.Count:N0} files will be restored.\n\n" +
                "The selected operation and every operation after it will be restored. " +
                "Earlier operations will remain unchanged.\n\n" +
                "This will physically move the files back to their original paths " +
                "and mark the restored FileChanges records as Restored.\n\n" +
                "Do you want to continue?";

            var confirmed =
                await RequestConfirmationAsync(
                    "Restore Selected Changes",
                    restoreConfirmationMessage,
                    "Restore Changes");

            if (!confirmed)
            {
                Status =
                    "Restore cancelled.";

                return;
            }

            // --------------------------------------------------------
            // Restore newest operation first.
            // --------------------------------------------------------

            var restoreOrder =
                operationsToRestore
                    .AsEnumerable()
                    .Reverse()
                    .SelectMany(operation =>
                        operation.Changes
                            .OrderByDescending(
                                change => change.ChangeId))
                    .ToList();

            var restoredChanges =
                new List<FileChangeRecord>();

            try
            {
                foreach (var change in restoreOrder)
                {
                    Status =
                        $"Restoring {restoredChanges.Count + 1:N0} of {restoreOrder.Count:N0}...";

                    // ------------------------------------------------
                    // Physical restore:
                    //
                    // NewPath -> OriginalPath
                    // ------------------------------------------------

                    MoveFile(
                        change.NewPath,
                        change.OriginalPath);

                    // ------------------------------------------------
                    // Confirm the physical restore completed.
                    // ------------------------------------------------

                    if (!File.Exists(change.OriginalPath) ||
                        File.Exists(change.NewPath))
                    {
                        throw new IOException(
                            $"The restored file could not be verified. " +
                            $"Original='{change.OriginalPath}', " +
                            $"New='{change.NewPath}'.");
                    }

                    // ------------------------------------------------
                    // Mark the FileChange as Restored.
                    // ------------------------------------------------

                    try
                    {
                        await App.Services
                            .FileChangeRepository
                            .UpdateStatusAsync(
                                change.ChangeId,
                                "Restored");
                    }
                    catch (Exception statusException)
                    {
                        try
                        {
                            MoveFile(
                                change.OriginalPath,
                                change.NewPath);
                        }
                        catch (Exception rollbackException)
                        {
                            throw new IOException(
                                $"CRITICAL: FileChange {change.ChangeId} was physically restored " +
                                $"but its status could not be updated, and physical rollback failed. " +
                                $"Original='{change.OriginalPath}', " +
                                $"New='{change.NewPath}'. " +
                                $"StatusError='{statusException.Message}'. " +
                                $"RollbackError='{rollbackException.Message}'.",
                                rollbackException);
                        }

                        throw new IOException(
                            $"FileChange {change.ChangeId} could not be marked as Restored. " +
                            "The physical restore was rolled back.",
                            statusException);
                    }

                    restoredChanges.Add(change);
                }
            }
            catch (Exception restoreException)
            {
                Debug.WriteLine(
                    $"Restore operation failed: {restoreException}");

                foreach (
                    var restoredChange in
                    restoredChanges.AsEnumerable().Reverse())
                {
                    try
                    {
                        if (File.Exists(restoredChange.OriginalPath) &&
                            !File.Exists(restoredChange.NewPath))
                        {
                            MoveFile(
                                restoredChange.OriginalPath,
                                restoredChange.NewPath);
                        }

                        await App.Services
                            .FileChangeRepository
                            .UpdateStatusAsync(
                                restoredChange.ChangeId,
                                "Completed");
                    }
                    catch (Exception rollbackException)
                    {
                        Debug.WriteLine(
                            $"CRITICAL: Restore rollback failed for ChangeId={restoredChange.ChangeId}. " +
                            $"Original='{restoredChange.OriginalPath}', " +
                            $"New='{restoredChange.NewPath}', " +
                            $"Error='{rollbackException}'");
                    }
                }

                Status =
                    "Restore failed. The application attempted to roll back the changes already restored.";

                OperationResult =
                    $"Restore failed after {restoredChanges.Count:N0} of " +
                    $"{restoreOrder.Count:N0} files.\n\n" +
                    restoreException.Message;

                return;
            }

            // --------------------------------------------------------
            // Successful restore.
            // --------------------------------------------------------

            OperationResult =
                $"{restoredChanges.Count:N0} files restored.\n" +
                $"{operationsToRestore.Count:N0} Improve operations restored.\n" +
                "Earlier Improve operations were left unchanged.";

            Status =
                "Restore completed successfully.";

            Debug.WriteLine(
                $"Improve restore completed successfully. " +
                $"RestorePoint={SelectedRestorePoint.OperationId}, " +
                $"OperationsRestored={operationsToRestore.Count}, " +
                $"ChangesRestored={restoredChanges.Count}.");

            // --------------------------------------------------------
            // Re-run Analysis against the physically restored library.
            // --------------------------------------------------------

            await RefreshAnalysisAfterDuplicateOperationAsync();

            // --------------------------------------------------------
            // Reload restore points.
            // --------------------------------------------------------

            await LoadRestorePointsAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Unexpected Improve restore failure: {exception}");

            Status =
                "Restore failed.";

            OperationResult =
                $"Restore failed.\n\n{exception.Message}";
        }
    }

    // ============================================================
    // Open DIASISS Duplicates Folder
    // ============================================================

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

        // --------------------------------------------------------
        // Number of files the user has asked Improve to move.
        // --------------------------------------------------------

        var selectedForMove =
            duplicateIssues.Sum(
                issue =>
                    issue.Results.Count -
                    issue.SelectedResultIds.Count);

        // --------------------------------------------------------
        // User confirmation BEFORE making any physical changes
        // or creating the destination folder.
        // --------------------------------------------------------

        var confirmationMessage =
            $"{selectedForMove:N0} files are selected to be moved to the DIASISS Duplicates folder.\n\n" +
            "This will physically move the files and record the changes so they can be restored later.\n\n" +
            "Do you want to continue?";

        var confirmed =
            await RequestConfirmationAsync(
                "Remove Duplicates",
                confirmationMessage,
                "Remove Duplicates");

        if (!confirmed)
        {
            Status =
                "Remove Duplicates cancelled.";

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

        foreach (var issue in duplicateIssues)
        {
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

                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    missingCount++;

                    Debug.WriteLine(
                        $"Duplicate result {result.Id} has no file path.");

                    continue;
                }

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

                    if (PathsEqual(
                        sourcePath,
                        destinationPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    MoveFile(
                        sourcePath,
                        destinationPath);

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
        // Operation Result
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

        Debug.WriteLine(
            $"Duplicate Improve complete. OperationId={operationId}, " +
            $"SelectedForMove={selectedForMove}, " +
            $"Moved={movedCount}, " +
            $"Missing={missingCount}, " +
            $"Failed={failedCount}, " +
            $"Skipped={skippedCount}.");

        // --------------------------------------------------------
        // Automatic post-operation Analysis refresh.
        // --------------------------------------------------------

        await RefreshAnalysisAfterDuplicateOperationAsync();

        // --------------------------------------------------------
        // Refresh restore points.
        // --------------------------------------------------------

        await LoadRestorePointsAsync();
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

            App.Services
                .AnalysisRepository
                .Save(analysis);

            UpdateCategoryCounts();

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
            Debug.WriteLine(
                $"Automatic Analysis refresh after Duplicate Improve failed: {exception}");

            OperationResult +=
                "\nAutomatic Analysis refresh failed. Please run Analyse Library manually.";
        }
    }

    // ============================================================
    // Get Unique Destination Path
    // ============================================================

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
            // File.Move can fail when source and destination are
            // located on different volumes. Fall through to copy/delete.
        }

        File.Copy(
            sourcePath,
            destinationPath,
            overwrite: false);

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

    [RelayCommand]
    private void Next()
    {
        App.Services
            .ApplicationState
            .NavigateTo(
                WorkspaceType.Structure);
    }

    // ============================================================
    // Restore Point Model
    // ============================================================

    /// <summary>
    /// Represents one completed Improve operation that can be used
    /// as a Restore Changes point.
    ///
    /// RestorePointInfo deliberately contains only information
    /// needed to identify and display the operation at this stage.
    /// It does not perform any restore operation itself.
    /// </summary>
    public sealed class RestorePointInfo
    {
        public string OperationId { get; }

        public DateTime ChangedDate { get; }

        public int ChangeCount { get; }

        public int OperationsToRestore { get; }

        public int ChangesToRestore { get; }

        public string DisplayDate =>
            ChangedDate == DateTime.MinValue
                ? "Unknown date"
                : ChangedDate
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm:ss");

        public string DisplaySummary =>
            $"{DisplayDate} — {ChangeCount:N0} changes";

        public string DisplayRestoreSummary =>
            $"{OperationsToRestore:N0} operations — {ChangesToRestore:N0} changes will be restored";

        public RestorePointInfo(
            string operationId,
            DateTime changedDate,
            int changeCount,
            int operationsToRestore,
            int changesToRestore)
        {
            OperationId = operationId;
            ChangedDate = changedDate;
            ChangeCount = changeCount;
            OperationsToRestore = operationsToRestore;
            ChangesToRestore = changesToRestore;
        }
    }
}