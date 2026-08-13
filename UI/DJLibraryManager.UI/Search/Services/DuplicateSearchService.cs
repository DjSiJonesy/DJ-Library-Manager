using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.Services.Media;
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
///
/// Physical files are inspected so Search can distinguish between
/// files that merely exist and files that can actually be opened
/// and inspected successfully.
///
/// Search does not modify the DIASISS library.
/// </summary>
public sealed class DuplicateSearchService : ISearchService
{
    private readonly LibraryRepository _libraryRepository;

    private readonly FileInspectionService
        _fileInspectionService = new();

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

        // --------------------------------------------------------
        // Physical file inspection cache
        //
        // A duplicate group can contain the same physical path
        // more than once. Avoid inspecting the same path twice
        // within this search operation.
        // --------------------------------------------------------

        var inspectionCache =
            new Dictionary<string, FileInspectionResult>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var media =
                await _libraryRepository.GetMediaItemAsync(
                    path);

            var inspection =
                await InspectFileAsync(
                    path,
                    inspectionCache,
                    cancellationToken);

            results.Add(
                CreateResult(
                    path,
                    media,
                    inspection));
        }

        // --------------------------------------------------------
        // Recommendation
        //
        // Determine the strongest candidate and then place that
        // candidate first in the results collection.
        //
        // The recommendation calculation itself is unchanged.
        // --------------------------------------------------------

        RecommendBestResult(results);

        return results;
    }

    // ============================================================
    // Physical File Inspection
    // ============================================================

    private async Task<FileInspectionResult> InspectFileAsync(
        string filePath,
        Dictionary<string, FileInspectionResult> cache,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(
                filePath,
                out var cached))
        {
            return cached;
        }

        var inspection =
            await _fileInspectionService.InspectAsync(
                filePath,
                cancellationToken);

        cache[filePath] =
            inspection;

        return inspection;
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
        DJLMMediaItem? media,
        FileInspectionResult inspection)
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
                    inspection.Exists,

                FileSize =
                    inspection.Exists
                        ? TryGetFileSize(filePath)
                        : null,

                // ------------------------------------------------
                // Physical File Inspection
                // ------------------------------------------------

                IsInspected =
                    true,

                IsHealthy =
                    inspection.IsHealthy,

                IntegrityStatus =
                    inspection.IntegrityStatus,

                Format =
                    inspection.Format,

                Codec =
                    inspection.Codec,

                IsLossless =
                    inspection.IsLossless,

                Bitrate =
                    inspection.Bitrate,

                SampleRate =
                    inspection.SampleRate,

                BitDepth =
                    inspection.BitDepth,

                Channels =
                    inspection.Channels
            };

        // ========================================================
        // Library Media Information
        // ========================================================

        if (media is not null)
        {
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
        }
        else
        {
            // ----------------------------------------------------
            // If the DIASISS library no longer contains the media
            // record, still retain the physical file information
            // discovered by inspection.
            // ----------------------------------------------------

            result.Duration =
                inspection.Duration;
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
    // File Size
    // ============================================================

    private static long? TryGetFileSize(
        string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            return new FileInfo(filePath).Length;
        }
        catch
        {
            return null;
        }
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

        // --------------------------------------------------------
        // Put the recommended result first.
        //
        // The remaining candidates retain their existing order.
        //
        // This is presentation ordering only. It does not alter
        // the duplicate group or the user's eventual selection.
        // --------------------------------------------------------

        var recommendedIndex =
            results.IndexOf(recommended);

        if (recommendedIndex > 0)
        {
            results.RemoveAt(
                recommendedIndex);

            results.Insert(
                0,
                recommended);
        }
    }

    // ============================================================
    // Current Scoring
    // ============================================================

    /// <summary>
    /// Calculates the current Search candidate score.
    ///
    /// IMPORTANT:
    /// This is deliberately the existing scoring algorithm.
    ///
    /// The physical inspection information is now available to
    /// Search, but it is NOT yet used to change the recommendation
    /// score. The quality/integrity scoring will be redesigned
    /// separately once the inspection results have been validated.
    /// </summary>
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

        // --------------------------------------------------------
        // Physical File
        // --------------------------------------------------------

        reasons.Add(
            "File exists");

        if (result.IsInspected == true)
        {
            if (result.IsHealthy == true)
            {
                reasons.Add(
                    "File inspection passed");
            }
            else
            {
                reasons.Add(
                    "File inspection failed");
            }
        }

        // --------------------------------------------------------
        // Audio Information
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                result.Format))
        {
            reasons.Add(
                $"{result.Format} format");
        }

        if (result.IsLossless == true)
        {
            reasons.Add(
                "Lossless audio");
        }
        else if (result.IsLossless == false)
        {
            reasons.Add(
                "Lossy audio");
        }

        if (result.Bitrate.HasValue &&
            result.Bitrate.Value > 0)
        {
            reasons.Add(
                $"Bitrate {result.Bitrate.Value / 1000:N0} kbps");
        }

        if (result.SampleRate.HasValue &&
            result.SampleRate.Value > 0)
        {
            reasons.Add(
                $"Sample rate {result.SampleRate.Value:N0} Hz");
        }

        if (result.BitDepth.HasValue &&
            result.BitDepth.Value > 0)
        {
            reasons.Add(
                $"{result.BitDepth.Value}-bit");
        }

        // --------------------------------------------------------
        // Metadata
        // --------------------------------------------------------

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