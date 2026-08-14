using DJLibraryManager.UI.Search.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Interfaces;

/// <summary>
/// Defines an external provider capable of finding metadata
/// for tracks investigated by the Search workflow.
///
/// Providers discover metadata only. They never modify the
/// DIASISS library.
/// </summary>
public interface IMetadataSearchProvider
{
    /// <summary>
    /// Friendly provider name displayed in Search results.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Searches the provider for possible metadata matches.
    /// </summary>
    Task<IReadOnlyList<MetadataSearchProviderResult>> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default);
}