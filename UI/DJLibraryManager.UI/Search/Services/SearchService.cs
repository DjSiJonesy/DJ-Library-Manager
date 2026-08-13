using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Coordinates Search operations and routes each issue to the
/// appropriate Search service.
///
/// Search investigates issues identified by Analysis.
/// It does not modify the DIASISS library.
///
/// Search All is owned by this service rather than the Search
/// workspace so that a long-running Search operation survives
/// navigation between application workspaces.
/// </summary>
public sealed class SearchService
{
    private const int SearchCheckpointInterval = 10;

    private readonly Dictionary<string, ISearchService>
        _services;

    private readonly SearchRepository _searchRepository;

    private readonly AnalysisRepository _analysisRepository;

    // ============================================================
    // State
    // ============================================================

    /// <summary>
    /// Indicates whether a Search All operation is currently
    /// running.
    /// </summary>
    public bool IsSearching { get; private set; }

    /// <summary>
    /// The currently running Search All operation.
    /// </summary>
    public SearchRun? CurrentRun { get; private set; }

    /// <summary>
    /// Raised whenever Search All progress changes.
    ///
    /// SearchWorkspaceViewModel subscribes to this event so the
    /// UI can update even when the Search workspace is recreated.
    /// </summary>
    public event EventHandler<SearchRun>? ProgressChanged;

    // ============================================================
    // Constructor
    // ============================================================

    public SearchService(
        DuplicateSearchService duplicateSearchService,
        MissingFileSearchService missingFileSearchService,
        MetadataSearchService metadataSearchService,
        MusicSearchService musicSearchService,
        ProviderSearchService providerSearchService,
        SearchRepository searchRepository,
        AnalysisRepository analysisRepository)
    {
        if (duplicateSearchService is null)
            throw new ArgumentNullException(
                nameof(duplicateSearchService));

        if (missingFileSearchService is null)
            throw new ArgumentNullException(
                nameof(missingFileSearchService));

        if (metadataSearchService is null)
            throw new ArgumentNullException(
                nameof(metadataSearchService));

        if (musicSearchService is null)
            throw new ArgumentNullException(
                nameof(musicSearchService));

        if (providerSearchService is null)
            throw new ArgumentNullException(
                nameof(providerSearchService));

        if (searchRepository is null)
            throw new ArgumentNullException(
                nameof(searchRepository));

        if (analysisRepository is null)
            throw new ArgumentNullException(
                nameof(analysisRepository));

        _searchRepository =
            searchRepository;

        _analysisRepository =
            analysisRepository;

        _services =
            new Dictionary<string, ISearchService>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["Duplicates"] =
                    duplicateSearchService,

                ["File Integrity"] =
                    missingFileSearchService,

                ["Metadata"] =
                    metadataSearchService,

                ["Music"] =
                    musicSearchService,

                ["Providers"] =
                    providerSearchService
            };
    }

    // ============================================================
    // Individual Search
    // ============================================================

    /// <summary>
    /// Searches an individual Search issue using the Search
    /// service registered for its category.
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        if (issue is null)
            throw new ArgumentNullException(
                nameof(issue));

        cancellationToken.ThrowIfCancellationRequested();

        if (!_services.TryGetValue(
                issue.Category,
                out var service))
        {
            return Array.Empty<SearchResult>();
        }

        return await service.SearchAsync(
            issue,
            cancellationToken);
    }

    // ============================================================
    // Search All
    // ============================================================

    /// <summary>
    /// Searches all issues in a Search category.
    ///
    /// The operation is owned by SearchService rather than the
    /// Search workspace, allowing it to continue while the user
    /// navigates to another workspace.
    ///
    /// Completed issues are skipped when an existing interrupted
    /// SearchRun is resumed.
    /// </summary>
    public async Task<SearchRun> SearchAllAsync(
        IReadOnlyList<SearchIssue> issues,
        string category,
        CancellationToken cancellationToken = default)
    {
        if (issues is null)
            throw new ArgumentNullException(
                nameof(issues));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException(
                "Search category is required.",
                nameof(category));

        if (IsSearching)
            throw new InvalidOperationException(
                "A Search All operation is already running.");

        var analysis =
            _analysisRepository
                .CurrentAnalysis;

        if (analysis is null)
            throw new InvalidOperationException(
                "No completed Analysis is available.");

        var categoryIssues =
            issues.ToList();

        if (categoryIssues.Count == 0)
        {
            var emptyRun =
                new SearchRun
                {
                    AnalysisDate =
                        analysis.AnalysisDate,

                    Category =
                        category,

                    Status =
                        "Completed",

                    StartedAt =
                        DateTime.Now,

                    CompletedAt =
                        DateTime.Now,

                    TotalIssues =
                        0,

                    IssuesSearched =
                        0,

                    IssuesWithResults =
                        0
                };

            CurrentRun =
                emptyRun;

            RaiseProgressChanged(
                emptyRun);

            return emptyRun;
        }

        // ========================================================
        // Look for an interrupted Search Run.
        // ========================================================

        var savedSearch =
            _searchRepository.CurrentSearch;

        SearchRun? run = null;

        if (savedSearch is not null &&
            savedSearch.AnalysisDate ==
                analysis.AnalysisDate &&
            savedSearch.Run is not null &&
            savedSearch.Run.Status == "Running" &&
            savedSearch.Run.Category.Equals(
                category,
                StringComparison.OrdinalIgnoreCase))
        {
            run =
                savedSearch.Run;
        }

        // ========================================================
        // Create a new run when there isn't a resumable one.
        // ========================================================

        if (run is null)
        {
            run =
                new SearchRun
                {
                    AnalysisDate =
                        analysis.AnalysisDate,

                    Category =
                        category,

                    Status =
                        "Running",

                    StartedAt =
                        DateTime.Now,

                    CompletedAt =
                        null,

                    TotalIssues =
                        categoryIssues.Count,

                    IssuesSearched =
                        categoryIssues.Count(
                            x => x.IsSearched),

                    IssuesWithResults =
                        categoryIssues.Count(
                            x => x.HasResults)
                };
        }
        else
        {
            // Recalculate from the actual loaded issues.
            //
            // This protects us from a previous application
            // shutdown occurring between checkpoints.

            run.Status =
                "Running";

            run.CompletedAt =
                null;

            run.TotalIssues =
                categoryIssues.Count;

            run.IssuesSearched =
                categoryIssues.Count(
                    x => x.IsSearched);

            run.IssuesWithResults =
                categoryIssues.Count(
                    x => x.HasResults);
        }

        CurrentRun =
            run;

        IsSearching = true;

        // ========================================================
        // Persist Running state BEFORE starting.
        // ========================================================

        SaveRun(
            run,
            categoryIssues,
            analysis);

        RaiseProgressChanged(
            run);

        var completedSinceCheckpoint = 0;

        try
        {
            foreach (var issue in categoryIssues)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // ------------------------------------------------
                // Already completed.
                // ------------------------------------------------

                if (issue.IsSearched)
                    continue;

                var results =
                    await SearchAsync(
                        issue,
                        cancellationToken);

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

                // ------------------------------------------------
                // Update progress.
                // ------------------------------------------------

                run.IssuesSearched =
                    categoryIssues.Count(
                        x => x.IsSearched);

                run.IssuesWithResults =
                    categoryIssues.Count(
                        x => x.HasResults);

                completedSinceCheckpoint++;

                RaiseProgressChanged(
                    run);

                // ------------------------------------------------
                // Checkpoint periodically.
                // ------------------------------------------------

                if (completedSinceCheckpoint >=
                    SearchCheckpointInterval)
                {
                    SaveRun(
                        run,
                        categoryIssues,
                        analysis);

                    completedSinceCheckpoint = 0;
                }
            }

            // ====================================================
            // Completed.
            // ====================================================

            run.IssuesSearched =
                categoryIssues.Count(
                    x => x.IsSearched);

            run.IssuesWithResults =
                categoryIssues.Count(
                    x => x.HasResults);

            run.Status =
                "Completed";

            run.CompletedAt =
                DateTime.Now;

            SaveRun(
                run,
                categoryIssues,
                analysis);

            RaiseProgressChanged(
                run);

            return run;
        }
        catch
        {
            // ====================================================
            // IMPORTANT:
            //
            // Leave Status as Running.
            //
            // This means an unexpected shutdown or interruption
            // leaves a resumable Search Run.
            // ====================================================

            run.Status =
                "Running";

            run.IssuesSearched =
                categoryIssues.Count(
                    x => x.IsSearched);

            run.IssuesWithResults =
                categoryIssues.Count(
                    x => x.HasResults);

            SaveRun(
                run,
                categoryIssues,
                analysis);

            RaiseProgressChanged(
                run);

            throw;
        }
        finally
        {
            IsSearching = false;
        }
    }

    // ============================================================
    // Save Run
    // ============================================================

    private void SaveRun(
        SearchRun run,
        IReadOnlyList<SearchIssue> issues,
        LibraryAnalysisResult analysis)
    {
        var existing =
            _searchRepository.CurrentSearch;

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

        foreach (var issue in issues)
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

                Run =
                    run,

                Issues =
                    persistedIssues.Values.ToList()
            };

        _searchRepository.Save(
            state);
    }

    // ============================================================
    // Progress
    // ============================================================

    private void RaiseProgressChanged(
        SearchRun run)
    {
        ProgressChanged?.Invoke(
            this,
            run);
    }
}