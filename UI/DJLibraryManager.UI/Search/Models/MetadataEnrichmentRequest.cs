using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Describes a request for additional metadata discovery after
/// the primary recording search has completed.
///
/// The request is based on the strongest recording identity that
/// DIASISS has established. It is only created for metadata fields
/// that remain unresolved after the primary search.
///
/// Provider identities allow enrichment providers to retrieve
/// metadata for an already identified recording without performing
/// another broad metadata search.
/// </summary>
public sealed class MetadataEnrichmentRequest
{
    /// <summary>
    /// Artist identity established by the primary search.
    /// </summary>
    public string Artist { get; init; } = string.Empty;

    /// <summary>
    /// Track title identity established by the primary search.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Album information established by the primary search,
    /// when available.
    /// </summary>
    public string Album { get; init; } = string.Empty;

    /// <summary>
    /// Provider-specific identities established during the
    /// primary search.
    ///
    /// These identify the exact external recording or release
    /// that should be used for enrichment.
    /// </summary>
    public IReadOnlyList<MetadataProviderIdentity>
        ProviderIdentities
    { get; init; }
        = Array.Empty<MetadataProviderIdentity>();

    /// <summary>
    /// Metadata fields that still require discovery.
    ///
    /// Examples:
    ///     Year
    ///     Genre
    /// </summary>
    public IReadOnlyList<string> MissingFields { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Path of the local media file being investigated.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Existing local duration, when available.
    ///
    /// This is supplied as contextual information only.
    /// Enrichment must not reject a metadata value simply because
    /// the local duration is unavailable.
    /// </summary>
    public TimeSpan? Duration { get; init; }
}