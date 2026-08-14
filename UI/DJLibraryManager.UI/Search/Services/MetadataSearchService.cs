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
/// Filename-derived information supplied by Analysis is treated as
/// search evidence only. It is never treated as confirmed metadata.
/// </summary>
public sealed class MetadataSearchService : ISearchService
{
    private readonly IReadOnlyList<IMetadataSearchProvider> _providers;

    /// <summary>
    /// Creates a metadata search service using the supplied providers.
    /// </summary>
    public MetadataSearchService(
        IEnumerable<IMetadataSearchProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers =
            providers.ToList();
    }

    /// <summary>
    /// Searches a metadata issue using all registered metadata
    /// providers.
    ///
    /// If Analysis supplied multiple filename interpretations,
    /// each interpretation is searched independently.
    ///
    /// Search only discovers possible metadata. It does not modify
    /// the DIASISS library or the physical media file.
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(issue);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(issue.FilePath))
        {
            return [];
        }

        // ========================================================
        // Determine Missing Metadata
        // ========================================================

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
        // Search Providers
        // ========================================================

        var results =
            new List<SearchResult>();

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
                    // A failure from one metadata provider must not
                    // prevent other providers or search
                    // interpretations from being searched.
                    continue;
                }

                foreach (var providerResult in providerResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    results.Add(
                        ConvertToSearchResult(
                            providerResult,
                            issue));
                }
            }
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
        //
        // Analysis has already determined the possible filename
        // interpretations.
        //
        // We do NOT attempt to interpret the filename again here.
        //

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

                // ------------------------------------------------
                // Don't send completely empty searches to a
                // provider.
                // ------------------------------------------------

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
        //
        // If Artist and/or Title already exist, make sure we also
        // search using the actual library metadata.
        //
        // This is particularly important when only Album, Genre,
        // BPM, Key, etc. are missing.
        //

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
        //
        // Analysis may have a filename hint but may not have been
        // able to split it into two meaningful parts.
        //
        // In that case, use the cleaned filename as the Title
        // search term.
        //

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
            requests.Add(request);
        }
    }

    // ============================================================
    // Convert Provider Result
    // ============================================================

    private static SearchResult ConvertToSearchResult(
        MetadataSearchProviderResult providerResult,
        SearchIssue issue)
    {
        return new SearchResult
        {
            Id =
                Guid.NewGuid().ToString(),

            Source =
                providerResult.Source,

            MatchScore =
                Math.Clamp(
                    providerResult.Confidence,
                    0,
                    100),

            IsRecommended =
                false,

            RecommendationReason =
                providerResult.MatchReason,

            Artist =
                providerResult.Artist,

            TrackTitle =
                providerResult.Title,

            Album =
                providerResult.Album,

            Genre =
                providerResult.Genre,

            Bpm =
                providerResult.BPM,

            Key =
                providerResult.Key,

            Duration =
                providerResult.Duration,

            FilePath =
                issue.FilePath,

            FileExists =
                File.Exists(
                    issue.FilePath),

            IntegrityStatus =
                string.Empty
        };
    }

    // ============================================================
    // Missing Metadata
    // ============================================================

    private static List<string> GetMissingMetadata(
        SearchIssue issue)
    {
        var missing =
            new List<string>();

        var type =
            issue.Type.Trim();

        if (type.Equals(
                "MissingArtist",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Artist");
        }

        if (type.Equals(
                "MissingTitle",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Title");
        }

        if (type.Equals(
                "MissingAlbum",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Album");
        }

        if (type.Equals(
                "MissingGenre",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Genre");
        }

        if (type.Equals(
                "MissingYear",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Year");
        }

        if (type.Equals(
                "MissingBPM",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("BPM");
        }

        if (type.Equals(
                "MissingKey",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Key");
        }

        if (type.Equals(
                "MissingDuration",
                StringComparison.OrdinalIgnoreCase))
        {
            missing.Add("Duration");
        }

        if (missing.Count == 0 &&
            !string.IsNullOrWhiteSpace(issue.Title))
        {
            missing.Add(
                issue.Title);
        }

        return missing;
    }
}