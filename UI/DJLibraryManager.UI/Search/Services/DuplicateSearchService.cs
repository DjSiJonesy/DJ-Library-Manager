using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Searches duplicate groups identified by the Analysis workflow.
///
/// This service does not identify duplicates itself. Analysis has
/// already established the duplicate group. Search evaluates the
/// files within that group and recommends the strongest candidate.
/// </summary>
public sealed class DuplicateSearchService : ISearchService
{
    private readonly LibraryRepository _libraryRepository;

    public DuplicateSearchService(
        LibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository
            ?? throw new ArgumentNullException(
                nameof(libraryRepository));
    }

    // ============================================================
    // Search
    // ============================================================

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        if (issue is null)
            throw new ArgumentNullException(
                nameof(issue));

        cancellationToken.ThrowIfCancellationRequested();

        var paths = new List<string>();

        AddPath(
            paths,
            issue.FilePath);

        foreach (var path in issue.RelatedFilePaths)
        {
            AddPath(
                paths,
                path);
        }

        var results =
            new List<SearchResult>();

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var media =
                await _libraryRepository.GetMediaItemAsync(
                    path);

            results.Add(
                CreateResult(
                    path,
                    media));
        }

        RecommendBestResult(results);

        return results;
    }

    // ============================================================
    // Path Handling
    // ============================================================

    private static void AddPath(
        List<string> paths,
        string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (paths.Any(
                x => string.Equals(
                    x,
                    path,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        paths.Add(path);
    }

    // ============================================================
    // Result Creation
    // ============================================================

    private static SearchResult CreateResult(
        string filePath,
        DJLMMediaItem? media)
    {
        var result =
            new SearchResult
            {
                Id =
                    Guid.NewGuid().ToString(),

                Source =
                    "DIASISS Library",

                FilePath =
                    filePath,

                FileExists =
                    File.Exists(filePath)
            };

        if (media is null)
        {
            result.MatchScore = 0;

            result.RecommendationReason =
                result.FileExists
                    ? "File exists, but the track is no longer present in the DIASISS library."
                    : "File is missing and the track is no longer present in the DIASISS library.";

            return result;
        }

        // ========================================================
        // Media Information
        // ========================================================

        result.Artist =
            media.Artist;

        result.TrackTitle =
            media.Title;

        result.Album =
            media.Album;

        result.Genre =
            media.Genre;

        result.Bpm =
            media.BPM;

        result.Key =
            media.Key;

        result.Duration =
            media.Duration;

        // ========================================================
        // File Information
        // ========================================================

        if (result.FileExists)
        {
            try
            {
                var fileInfo =
                    new FileInfo(filePath);

                result.FileSize =
                    fileInfo.Length;
            }
            catch
            {
                result.FileExists = false;
            }
        }

        // ========================================================
        // Initial Score
        // ========================================================

        result.MatchScore =
            CalculateMatchScore(result);

        result.RecommendationReason =
            BuildRecommendationReason(result);

        return result;
    }

    // ============================================================
    // Recommendation
    // ============================================================

    private static void RecommendBestResult(
        List<SearchResult> results)
    {
        if (results.Count == 0)
            return;

        var recommended =
            results
                .Where(x => x.FileExists)
                .OrderByDescending(x => x.MatchScore)
                .ThenByDescending(x => x.FileSize ?? 0)
                .FirstOrDefault();

        if (recommended is null)
            return;

        recommended.IsRecommended = true;

        recommended.RecommendationReason =
            BuildRecommendationReason(
                recommended,
                true);
    }

    // ============================================================
    // Scoring
    // ============================================================

    private static double CalculateMatchScore(
        SearchResult result)
    {
        if (!result.FileExists)
            return 0;

        var score = 0.0;

        // --------------------------------------------------------
        // File exists
        // --------------------------------------------------------

        score += 40;

        // --------------------------------------------------------
        // Core metadata
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(result.Artist))
            score += 12;

        if (!string.IsNullOrWhiteSpace(result.TrackTitle))
            score += 12;

        if (!string.IsNullOrWhiteSpace(result.Album))
            score += 6;

        if (!string.IsNullOrWhiteSpace(result.Genre))
            score += 6;

        // --------------------------------------------------------
        // Musical information
        // --------------------------------------------------------

        if (result.Bpm.HasValue &&
            result.Bpm.Value > 0 &&
            result.Bpm.Value <= 300)
        {
            score += 6;
        }

        if (!string.IsNullOrWhiteSpace(result.Key))
            score += 6;

        // --------------------------------------------------------
        // Duration
        // --------------------------------------------------------

        if (result.Duration.HasValue &&
            result.Duration.Value > TimeSpan.Zero)
        {
            score += 6;
        }

        return Math.Round(
            Math.Clamp(
                score,
                0,
                100),
            1);
    }

    // ============================================================
    // Recommendation Reason
    // ============================================================

    private static string BuildRecommendationReason(
        SearchResult result,
        bool recommended = false)
    {
        if (!result.FileExists)
        {
            return "File is missing.";
        }

        var reasons =
            new List<string>();

        reasons.Add(
            "File exists");

        if (!string.IsNullOrWhiteSpace(
                result.Artist))
        {
            reasons.Add(
                "Artist information available");
        }

        if (!string.IsNullOrWhiteSpace(
                result.TrackTitle))
        {
            reasons.Add(
                "Title information available");
        }

        if (!string.IsNullOrWhiteSpace(
                result.Album))
        {
            reasons.Add(
                "Album information available");
        }

        if (!string.IsNullOrWhiteSpace(
                result.Genre))
        {
            reasons.Add(
                "Genre information available");
        }

        if (result.Bpm.HasValue &&
            result.Bpm.Value > 0 &&
            result.Bpm.Value <= 300)
        {
            reasons.Add(
                "Valid BPM");
        }

        if (!string.IsNullOrWhiteSpace(
                result.Key))
        {
            reasons.Add(
                "Key available");
        }

        if (result.Duration.HasValue &&
            result.Duration.Value > TimeSpan.Zero)
        {
            reasons.Add(
                "Valid duration");
        }

        var prefix =
            recommended
                ? "Recommended because: "
                : "Candidate because: ";

        return prefix +
               string.Join(
                   " • ",
                   reasons);
    }
}