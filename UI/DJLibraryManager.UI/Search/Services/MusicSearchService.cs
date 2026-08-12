using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Searches for information relating to Music analysis issues.
///
/// Search investigates BPM, Key and Duration information already
/// available in the DIASISS library. It does not modify the library.
/// </summary>
public sealed class MusicSearchService : ISearchService
{
    private readonly LibraryRepository _libraryRepository;

    public MusicSearchService(
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

        if (string.IsNullOrWhiteSpace(issue.FilePath))
            return Array.Empty<SearchResult>();

        var media =
            await _libraryRepository.GetMediaItemAsync(
                issue.FilePath);

        cancellationToken.ThrowIfCancellationRequested();

        var result =
            CreateResult(
                issue.FilePath,
                media,
                issue.Type);

        return new[]
        {
            result
        };
    }

    // ============================================================
    // Result Creation
    // ============================================================

    private static SearchResult CreateResult(
        string filePath,
        DJLMMediaItem? media,
        string issueType)
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
        // Score
        // ========================================================

        result.MatchScore =
            CalculateMatchScore(
                result,
                issueType);

        result.RecommendationReason =
            BuildRecommendationReason(
                result,
                issueType);

        result.IsRecommended =
            result.FileExists &&
            result.MatchScore > 0;

        return result;
    }

    // ============================================================
    // Scoring
    // ============================================================

    private static double CalculateMatchScore(
        SearchResult result,
        string issueType)
    {
        if (!result.FileExists)
            return 0;

        var score = 0.0;

        // --------------------------------------------------------
        // File exists
        // --------------------------------------------------------

        score += 40;

        // --------------------------------------------------------
        // Artist / Title
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(result.Artist))
            score += 10;

        if (!string.IsNullOrWhiteSpace(result.TrackTitle))
            score += 10;

        // --------------------------------------------------------
        // BPM
        // --------------------------------------------------------

        if (result.Bpm.HasValue &&
            result.Bpm.Value > 0 &&
            result.Bpm.Value <= 300)
        {
            score += 10;
        }

        // --------------------------------------------------------
        // Key
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(result.Key))
            score += 10;

        // --------------------------------------------------------
        // Duration
        // --------------------------------------------------------

        if (result.Duration.HasValue &&
            result.Duration.Value > TimeSpan.Zero)
        {
            score += 10;
        }

        return Math.Round(
            Math.Clamp(
                score,
                0,
                100),
            1);
    }

    // ============================================================
    // Recommendation
    // ============================================================

    private static string BuildRecommendationReason(
        SearchResult result,
        string issueType)
    {
        if (!result.FileExists)
        {
            return "File is missing.";
        }

        var reasons =
            new List<string>();

        switch (issueType)
        {
            case "InvalidBPM":

                if (result.Bpm.HasValue &&
                    result.Bpm.Value > 0 &&
                    result.Bpm.Value <= 300)
                {
                    reasons.Add(
                        "Valid BPM is available");
                }
                else
                {
                    reasons.Add(
                        "No valid BPM is currently available");
                }

                break;

            case "InvalidKey":

                if (!string.IsNullOrWhiteSpace(result.Key))
                {
                    reasons.Add(
                        "Musical Key information is available");
                }
                else
                {
                    reasons.Add(
                        "No Musical Key is currently available");
                }

                break;

            case "InvalidDuration":

                if (result.Duration.HasValue &&
                    result.Duration.Value > TimeSpan.Zero)
                {
                    reasons.Add(
                        "Valid Duration is available");
                }
                else
                {
                    reasons.Add(
                        "No valid Duration is currently available");
                }

                break;

            default:

                if (result.Bpm.HasValue &&
                    result.Bpm.Value > 0 &&
                    result.Bpm.Value <= 300)
                {
                    reasons.Add(
                        "Valid BPM available");
                }

                if (!string.IsNullOrWhiteSpace(result.Key))
                {
                    reasons.Add(
                        "Key available");
                }

                if (result.Duration.HasValue &&
                    result.Duration.Value > TimeSpan.Zero)
                {
                    reasons.Add(
                        "Valid Duration available");
                }

                break;
        }

        if (reasons.Count == 0)
        {
            return "No additional Music information is currently available.";
        }

        return string.Join(
            " • ",
            reasons);
    }
}