using DJLibraryManager.UI.Models.Search;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Interfaces;

/// <summary>
/// Defines the contract for a Search operation.
///
/// Search investigates issues identified by Analysis and returns
/// possible solutions or candidates. Search does not modify the
/// DIASISS library.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches for possible solutions to the supplied issue.
    /// </summary>
    /// <param name="issue">
    /// The issue identified by the Analysis workflow.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the search.
    /// </param>
    /// <returns>
    /// A collection of possible search results.
    /// </returns>
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default);
}