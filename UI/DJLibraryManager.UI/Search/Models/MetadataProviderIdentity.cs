namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Identifies a specific recording or release within an
/// external metadata provider.
///
/// Provider identities are discovery data only. They allow
/// later enrichment operations to retrieve additional metadata
/// for an already established recording without performing
/// another broad metadata search.
/// </summary>
public sealed class MetadataProviderIdentity
{
    /// <summary>
    /// Name of the external metadata provider.
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Provider-specific identifier for the identified entity.
    /// </summary>
    public string ExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Type of entity represented by the identifier.
    ///
    /// Examples include:
    /// Recording, Release, Master, Track.
    /// </summary>
    public string EntityType { get; init; } = string.Empty;
}