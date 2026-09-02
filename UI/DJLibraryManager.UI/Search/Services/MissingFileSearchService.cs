using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Investigates files identified by Analysis as missing.
///
/// Search does not modify or move files. It only reports the
/// current state of the missing-file issue so that a future
/// Improve workflow can decide what action should be taken.
/// </summary>
public sealed class MissingFileSearchService : ISearchService
{
    /// <summary>
    /// Searches a missing-file issue.
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

        var fileExists =
            File.Exists(issue.FilePath);

        var result =
            new SearchResult
            {
                Id = Guid.NewGuid().ToString(),

                MediaId = issue.MediaId,

                Source = "Local Library",

                FilePath = issue.FilePath,

                FileExists = fileExists,

                MatchScore =
                    fileExists
                        ? 100
                        : 0,

                IsRecommended =
                    fileExists,

                RecommendationReason =
                    fileExists
                        ? "The file has returned to its original library location."
                        : "The original file could not be found at its recorded location."
            };

        results.Add(result);

        return Task.FromResult<
            IReadOnlyList<SearchResult>>(
                results);
    }
}