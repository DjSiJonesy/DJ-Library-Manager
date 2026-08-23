using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Performs the second-stage metadata enrichment search.
///
/// The primary metadata search is responsible for identifying
/// the recording. This service only searches for fields that remain
/// unresolved after the primary search.
///
/// Providers are queried independently. Results from one provider
/// are never supplied to another provider.
///
/// The service does not modify the DIASISS library.
/// </summary>
public sealed class MetadataEnrichmentService
{
    private readonly IReadOnlyList<IMetadataEnrichmentProvider>
        _providers;

    public MetadataEnrichmentService(
        IEnumerable<IMetadataEnrichmentProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers =
            providers
                .Where(
                    provider =>
                        provider is not null)
                .ToList();
    }

    /// <summary>
    /// Searches independently across all registered enrichment
    /// providers.
    /// </summary>
    public async Task<IReadOnlyList<MetadataSearchProviderResult>>
        EnrichAsync(
            MetadataEnrichmentRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (request.MissingFields is null ||
            request.MissingFields.Count == 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(request.Artist) &&
            string.IsNullOrWhiteSpace(request.Title))
        {
            return [];
        }

        var results =
            new List<MetadataSearchProviderResult>();

        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var providerResults =
                    await provider.EnrichAsync(
                        request,
                        cancellationToken);

                if (providerResults is null)
                {
                    continue;
                }

                foreach (var result in providerResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (result is null)
                    {
                        continue;
                    }

                    results.Add(result);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // One enrichment provider failing must not prevent
                // other enrichment providers from being searched.
                continue;
            }
        }

        return results;
    }
}