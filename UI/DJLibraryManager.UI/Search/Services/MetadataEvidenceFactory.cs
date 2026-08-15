using DJLibraryManager.UI.Search.Models;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Converts provider-specific search results into the common
/// DIASISS metadata evidence model.
///
/// This factory performs no matching, scoring or recommendation.
/// It simply transfers the information returned by a provider
/// into the provider-independent evidence model.
/// </summary>
public static class MetadataEvidenceFactory
{
    /// <summary>
    /// Converts a provider search result into metadata evidence.
    /// </summary>
    /// <param name="result">
    /// The result returned by an external metadata provider.
    /// </param>
    /// <returns>
    /// Provider-independent metadata evidence.
    /// </returns>
    public static MetadataEvidence Create(
        MetadataSearchProviderResult result)
    {
        return new MetadataEvidence
        {
            Source =
                result.Source,

            ExternalId =
                result.ExternalId,

            Artist =
                result.Artist,

            Title =
                result.Title,

            Album =
                result.Album,

            Genre =
                result.Genre,

            Year =
                result.Year,

            ReleaseYear =
                result.ReleaseYear,

            BPM =
                result.BPM,

            Key =
                result.Key,

            Duration =
                result.Duration,

            ProviderConfidence =
                result.Confidence,

            MatchReason =
                result.MatchReason
        };
    }
}