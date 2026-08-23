using DJLibraryManager.UI.Search.Models;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Interfaces;

/// <summary>
/// Provides additional metadata discovery after a recording has
/// already been identified by the primary metadata search.
///
/// Enrichment providers do not determine recording identity.
/// They receive an established Artist/Title identity and search
/// specifically for additional metadata fields that remain missing.
///
/// Enrichment providers do not modify the DIASISS library.
/// </summary>
public interface IMetadataEnrichmentProvider
{
    /// <summary>
    /// Name of the external metadata provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Searches for additional metadata for an already identified
    /// recording.
    /// </summary>
    Task<IReadOnlyList<MetadataSearchProviderResult>>
        EnrichAsync(
            MetadataEnrichmentRequest request,
            CancellationToken cancellationToken = default);
}