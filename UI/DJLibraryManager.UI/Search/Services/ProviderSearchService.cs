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
/// Investigates Provider issues identified by the Analysis workflow.
///
/// Provider Search examines the provider information already known
/// by DIASISS and reports the current state.
///
/// It does not modify provider databases or library records.
/// </summary>
public sealed class ProviderSearchService : ISearchService
{
    private readonly LibraryRepository _libraryRepository;

    public ProviderSearchService(
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

        return new[]
        {
            CreateResult(
                issue,
                media)
        };
    }

    // ============================================================
    // Result Creation
    // ============================================================

    private static SearchResult CreateResult(
        SearchIssue issue,
        DJLMMediaItem? media)
    {
        var result =
            new SearchResult
            {
                Id =
                    Guid.NewGuid().ToString(),

                MediaId =
                media?.MediaId ??
                string.Empty,

                Source =
                    "DIASISS Library",

                FilePath =
                    issue.FilePath,

                FileExists =
                    File.Exists(issue.FilePath)
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
                    new FileInfo(issue.FilePath);

                result.FileSize =
                    fileInfo.Length;
            }
            catch
            {
                result.FileExists = false;
            }
        }

        // ========================================================
        // Provider Information
        // ========================================================

        var provider =
            media.Provider?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(provider))
        {
            result.MatchScore =
                result.FileExists
                    ? 50
                    : 0;

            result.RecommendationReason =
                result.FileExists
                    ? "File exists, but no provider association is currently available."
                    : "File is missing and no provider association is currently available.";

            return result;
        }

        result.MatchScore =
            result.FileExists
                ? 100
                : 0;

        result.IsRecommended =
            result.FileExists;

        result.RecommendationReason =
            result.FileExists
                ? $"Provider association available: {provider}."
                : $"Provider association available ({provider}), but the file is missing.";

        return result;
    }
}