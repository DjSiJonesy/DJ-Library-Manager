using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Investigates metadata issues identified by the Analysis workflow.
///
/// Search identifies what metadata is missing and prepares the issue
/// for metadata discovery. It does not modify the DIASISS library.
/// </summary>
public sealed class MetadataSearchService : ISearchService
{
    /// <summary>
    /// Searches a metadata issue.
    /// </summary>
    public Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        if (issue is null)
            throw new ArgumentNullException(
                nameof(issue));

        cancellationToken.ThrowIfCancellationRequested();

        var results =
            new List<SearchResult>();

        if (string.IsNullOrWhiteSpace(issue.FilePath))
        {
            return Task.FromResult<
                IReadOnlyList<SearchResult>>(
                    results);
        }

        var missingMetadata =
            GetMissingMetadata(issue);

        var result =
            new SearchResult
            {
                Id = Guid.NewGuid().ToString(),

                Source = "Metadata Search",

                FilePath = issue.FilePath,

                FileExists = true,

                MatchScore = 0,

                IsRecommended = false,

                RecommendationReason =
                    missingMetadata.Count == 0
                        ? "No specific missing metadata field was identified."
                        : $"Missing metadata: {string.Join(", ", missingMetadata)}"
            };

        results.Add(result);

        return Task.FromResult<
            IReadOnlyList<SearchResult>>(
                results);
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

        if (missing.Count == 0)
        {
            missing.Add(
                issue.Title);
        }

        return missing;
    }
}