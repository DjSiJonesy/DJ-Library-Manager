using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Coordinates Search operations and routes each issue to the
/// appropriate Search service.
///
/// Search investigates issues identified by Analysis.
/// It does not modify the DIASISS library.
/// </summary>
public sealed class SearchService
{
    private readonly Dictionary<string, ISearchService>
        _services;

    public SearchService(
        DuplicateSearchService duplicateSearchService,
        MissingFileSearchService missingFileSearchService,
        MetadataSearchService metadataSearchService,
        MusicSearchService musicSearchService,
        ProviderSearchService providerSearchService)
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
}