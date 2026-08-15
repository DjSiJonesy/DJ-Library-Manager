using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Investigates metadata issues identified by the Analysis workflow.
///
/// Search identifies what metadata can potentially be recovered from
/// registered external metadata providers. It does not modify the
/// DIASISS library.
///
/// Every provider is queried independently using the original
/// SearchIssue information. Provider results are never passed to
/// another provider.
///
/// Provider results are converted into common DIASISS evidence,
/// independently matched against the original library track,
/// analysed for consensus, and finally converted into metadata
/// change recommendations.
///
/// Metadata recommendations are stored on the SearchIssue.
///
/// Provider SearchResult objects remain available as supporting
/// evidence, but are no longer treated as the thing the user
/// must choose between.
///
/// Search does not modify the DIASISS library or physical media.
/// </summary>
public sealed class MetadataSearchService : ISearchService
{
    private readonly IReadOnlyList<IMetadataSearchProvider> _providers;

    private readonly MetadataEvidenceAnalysisService
        _evidenceAnalysisService;

    private readonly MetadataConsensusService
        _consensusService;

    private readonly MetadataRecommendationService
        _recommendationService;

    /// <summary>
    /// Creates a metadata search service using the supplied providers.
    /// </summary>
    public MetadataSearchService(
        IEnumerable<IMetadataSearchProvider> providers,
        MetadataEvidenceAnalysisService? evidenceAnalysisService = null,
        MetadataConsensusService? consensusService = null,
        MetadataRecommendationService? recommendationService = null)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers =
            providers.ToList();

        _evidenceAnalysisService =
            evidenceAnalysisService ??
            new MetadataEvidenceAnalysisService();

        _consensusService =
            consensusService ??
            new MetadataConsensusService();

        _recommendationService =
            recommendationService ??
            new MetadataRecommendationService();
    }

    // ============================================================
    // Search
    // ============================================================

    /// <summary>
    /// Searches a metadata issue using all registered metadata
    /// providers independently.
    ///
    /// Provider results are converted into common DIASISS evidence,
    /// independently matched against the original library track,
    /// and then analysed collectively for metadata consensus.
    ///
    /// Metadata recommendations are attached to the supplied
    /// SearchIssue as workflow state.
    ///
    /// Search does not modify the DIASISS library or physical media.
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(issue);

        cancellationToken.ThrowIfCancellationRequested();

        // ========================================================
        // Clear Previous Recommendations
        // ========================================================

        issue.MetadataRecommendations.Clear();

        if (string.IsNullOrWhiteSpace(issue.FilePath))
        {
            return [];
        }

        // ========================================================
        // Determine Missing Metadata
        // ========================================================
        //
        // Analysis is authoritative here.
        //
        // Search must not attempt to reconstruct the missing fields
        // from the issue Type because AnalysisIssue already carries
        // the exact MissingFields collection.
        //

        var missingMetadata =
            GetMissingMetadata(issue);

        // ========================================================
        // Build Search Requests
        // ========================================================

        var requests =
            BuildSearchRequests(
                issue,
                missingMetadata);

        if (requests.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Build Original Local Track
        // ========================================================
        //
        // This represents what DIASISS currently knows.
        //
        // Filename-derived values are deliberately NOT copied here.
        // They remain search hypotheses and are supplied separately
        // to the candidate matcher.
        //

        var localMedia =
            CreateLocalMediaItem(
                issue);

        // ========================================================
        // Collect Provider Evidence
        // ========================================================
        //
        // Every provider receives the original request independently.
        //
        // Provider A does not see Provider B's result.
        //

        var evidence =
            new List<MetadataEvidence>();

        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var provider in _providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<MetadataSearchProviderResult>
                    providerResults;

                try
                {
                    providerResults =
                        await provider.SearchAsync(
                            request,
                            cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    // One provider failing must not prevent the
                    // remaining providers from being searched.
                    continue;
                }

                if (providerResults is null)
                {
                    continue;
                }

                foreach (var providerResult in providerResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (providerResult is null)
                    {
                        continue;
                    }

                    evidence.Add(
                        MetadataEvidenceFactory.Create(
                            providerResult));
                }
            }
        }

        if (evidence.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Candidate Analysis
        // ========================================================
        //
        // Every evidence item is independently compared against
        // the ORIGINAL local track.
        //
        // When Artist/Title are missing, the filename search hint
        // is supplied separately as a search hypothesis.
        //

        var analysedCandidates =
            _evidenceAnalysisService.Analyse(
                localMedia,
                evidence,
                issue.FilenameSearchHint);

        var viableCandidates =
            analysedCandidates
                .Where(
                    x =>
                        x.Match.IsMatch)
                .OrderByDescending(
                    x =>
                        x.Match.Score)
                .ToList();

        if (viableCandidates.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Consensus
        // ========================================================
        //
        // Consensus happens only after independent candidate
        // analysis has completed.
        //

        var consensusResults =
            _consensusService.Analyse(
                viableCandidates);

        // ========================================================
        // Metadata Recommendations
        // ========================================================
        //
        // These recommendations describe proposed metadata changes.
        //
        // They are NOT provider selections.
        //
        // They do NOT modify the library.
        //

        var recommendations =
            _recommendationService.Recommend(
                consensusResults);

        AddRecommendationsToIssue(
            issue,
            recommendations);

        // ========================================================
        // Provider Evidence Results
        // ========================================================
        //
        // SearchResult objects remain available as supporting
        // evidence.
        //
        // No provider result is marked as the overall metadata
        // recommendation.
        //

        return CreateSearchResults(
            viableCandidates,
            issue);
    }

    // ============================================================
    // Local Media
    // ============================================================

    private static DJLMMediaItem CreateLocalMediaItem(
        SearchIssue issue)
    {
        long fileSize = 0;

        try
        {
            if (!string.IsNullOrWhiteSpace(issue.FilePath) &&
                File.Exists(issue.FilePath))
            {
                fileSize =
                    new FileInfo(
                        issue.FilePath).Length;
            }
        }
        catch
        {
            // File size is not required for candidate matching.
        }

        return new DJLMMediaItem
        {
            Provider =
                "DIASISS",

            FilePath =
                issue.FilePath,

            FileSize =
                fileSize,

            Artist =
                issue.Artist?.Trim()
                ?? string.Empty,

            Title =
                issue.TrackTitle?.Trim()
                ?? string.Empty,

            Album =
                issue.Album?.Trim()
                ?? string.Empty,

            Genre =
                issue.Genre?.Trim()
                ?? string.Empty,

            Year =
                issue.Year,

            BPM =
                issue.Bpm,

            Key =
                issue.Key?.Trim()
                ?? string.Empty,

            Duration =
                issue.Duration
        };
    }

    // ============================================================
    // Metadata Recommendations
    // ============================================================

    /// <summary>
    /// Attaches metadata change recommendations to the SearchIssue.
    ///
    /// The current value comes from the original DIASISS library
    /// metadata represented by SearchIssue.
    ///
    /// Provider evidence is never treated as the current value.
    /// </summary>
    private static void AddRecommendationsToIssue(
    SearchIssue issue,
    IReadOnlyList<MetadataChangeRecommendation>
        recommendations)
    {
        foreach (var recommendation in recommendations)
        {
            if (recommendation is null)
            {
                continue;
            }

            var currentValue =
                GetCurrentMetadataValue(
                    issue,
                    recommendation.Field);

            // ----------------------------------------------------
            // Do not show a recommendation when the library
            // already contains exactly the recommended value.
            // ----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    recommendation.RecommendedValue) &&
                string.Equals(
                    currentValue.Trim(),
                    recommendation.RecommendedValue.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            issue.MetadataRecommendations.Add(
                new MetadataChangeRecommendation
                {
                    Field =
                        recommendation.Field,

                    CurrentValue =
                        currentValue,

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
                        false,

                    Reason =
                        recommendation.Reason
                });
        }
    }    

    // ============================================================
    // Current Metadata
    // ============================================================

    /// <summary>
    /// Returns the metadata currently stored in the SearchIssue.
    ///
    /// This deliberately reads only local DIASISS metadata.
    /// Provider evidence is never used as the current value.
    /// </summary>
    private static string GetCurrentMetadataValue(
        SearchIssue issue,
        string field)
    {
        if (field.Equals(
                "Artist",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Artist?.Trim()
                ?? string.Empty;
        }

        if (field.Equals(
                "Title",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.TrackTitle?.Trim()
                ?? string.Empty;
        }

        if (field.Equals(
                "Album",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Album?.Trim()
                ?? string.Empty;
        }

        if (field.Equals(
                "Genre",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Genre?.Trim()
                ?? string.Empty;
        }

        if (field.Equals(
                "Year",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Year?.ToString()
                ?? string.Empty;
        }

        if (field.Equals(
                "BPM",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Bpm?.ToString("0.##")
                ?? string.Empty;
        }

        if (field.Equals(
                "Key",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Key?.Trim()
                ?? string.Empty;
        }

        if (field.Equals(
                "Duration",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Duration.HasValue
                ? issue.Duration.Value.ToString(
                    @"mm\:ss")
                : string.Empty;
        }

        return string.Empty;
    }

    // ============================================================
    // Search Results
    // ============================================================

    private static IReadOnlyList<SearchResult> CreateSearchResults(
        IReadOnlyList<MetadataEvidenceAnalysisResult>
            viableCandidates,
        SearchIssue issue)
    {
        var results =
            new List<SearchResult>();

        foreach (var candidate in viableCandidates)
        {
            var evidence =
                candidate.Evidence;

            results.Add(
                new SearchResult
                {
                    Id =
                        Guid.NewGuid().ToString(),

                    Source =
                        evidence.Source,

                    MatchScore =
                        Math.Clamp(
                            candidate.Match.Score,
                            0,
                            100),

                    // ------------------------------------------------
                    // A provider candidate is not the overall
                    // metadata recommendation.
                    //
                    // Metadata recommendations are held by:
                    //
                    //     issue.MetadataRecommendations
                    // ------------------------------------------------

                    IsRecommended =
                        false,

                    RecommendationReason =
                        candidate.Match.Reason,

                    Artist =
                        evidence.Artist,

                    TrackTitle =
                        evidence.Title,

                    Album =
                        evidence.Album,

                    Genre =
                        evidence.Genre,

                    Bpm =
                        evidence.BPM,

                    Key =
                        evidence.Key,

                    Duration =
                        evidence.Duration,

                    FilePath =
                        issue.FilePath,

                    FileExists =
                        File.Exists(
                            issue.FilePath),

                    IntegrityStatus =
                        string.Empty
                });
        }

        return results;
    }

    // ============================================================
    // Build Search Requests
    // ============================================================

    private static List<MetadataSearchRequest>
        BuildSearchRequests(
            SearchIssue issue,
            IReadOnlyList<string> missingMetadata)
    {
        var requests =
            new List<MetadataSearchRequest>();

        var existingArtist =
            issue.Artist?.Trim()
            ?? string.Empty;

        var existingTitle =
            issue.TrackTitle?.Trim()
            ?? string.Empty;

        var hint =
            issue.FilenameSearchHint;

        // ========================================================
        // Filename Candidates
        // ========================================================

        if (hint is not null &&
            hint.Candidates.Count > 0)
        {
            foreach (var candidate in
                     hint.Candidates)
            {
                var artist =
                    !string.IsNullOrWhiteSpace(existingArtist)
                        ? existingArtist
                        : candidate.Artist?.Trim()
                            ?? string.Empty;

                var title =
                    !string.IsNullOrWhiteSpace(existingTitle)
                        ? existingTitle
                        : candidate.Title?.Trim()
                            ?? string.Empty;

                if (string.IsNullOrWhiteSpace(artist) &&
                    string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                AddRequestIfUnique(
                    requests,
                    new MetadataSearchRequest
                    {
                        Artist =
                            artist,

                        Title =
                            title,

                        Album =
                            issue.Album?.Trim()
                            ?? string.Empty,

                        FilePath =
                            issue.FilePath,

                        Duration =
                            issue.Duration,

                        MissingFields =
                            missingMetadata,

                        FilenameSearchHint =
                            hint
                    });
            }
        }

        // ========================================================
        // Normal Metadata Search
        // ========================================================

        if (!string.IsNullOrWhiteSpace(existingArtist) ||
            !string.IsNullOrWhiteSpace(existingTitle))
        {
            AddRequestIfUnique(
                requests,
                new MetadataSearchRequest
                {
                    Artist =
                        existingArtist,

                    Title =
                        existingTitle,

                    Album =
                        issue.Album?.Trim()
                        ?? string.Empty,

                    FilePath =
                        issue.FilePath,

                    Duration =
                        issue.Duration,

                    MissingFields =
                        missingMetadata,

                    FilenameSearchHint =
                        hint
                });
        }

        // ========================================================
        // Filename With No Candidate
        // ========================================================

        if (requests.Count == 0 &&
            hint is not null &&
            !string.IsNullOrWhiteSpace(
                hint.CleanedFilename))
        {
            requests.Add(
                new MetadataSearchRequest
                {
                    Artist =
                        existingArtist,

                    Title =
                        existingTitle.Length > 0
                            ? existingTitle
                            : hint.CleanedFilename,

                    Album =
                        issue.Album?.Trim()
                        ?? string.Empty,

                    FilePath =
                        issue.FilePath,

                    Duration =
                        issue.Duration,

                    MissingFields =
                        missingMetadata,

                    FilenameSearchHint =
                        hint
                });
        }

        return requests;
    }

    // ============================================================
    // Request Deduplication
    // ============================================================

    private static void AddRequestIfUnique(
        List<MetadataSearchRequest> requests,
        MetadataSearchRequest request)
    {
        var duplicate =
            requests.Any(
                existing =>
                    string.Equals(
                        existing.Artist,
                        request.Artist,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        existing.Title,
                        request.Title,
                        StringComparison.OrdinalIgnoreCase));

        if (!duplicate)
        {
            requests.Add(
                request);
        }
    }

    // ============================================================
    // Missing Metadata
    // ============================================================

    private static List<string> GetMissingMetadata(
        SearchIssue issue)
    {
        //
        // Analysis is the authority.
        //
        // SearchIssue.MissingFields was populated from AnalysisIssue
        // and must be preserved exactly.
        //

        if (issue.MissingFields is null ||
            issue.MissingFields.Count == 0)
        {
            return [];
        }

        return issue.MissingFields
            .Where(
                field =>
                    !string.IsNullOrWhiteSpace(field))
            .Select(
                field =>
                    field.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}