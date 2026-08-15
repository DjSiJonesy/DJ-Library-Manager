using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents a metadata value recommended by DIASISS based on
/// independently gathered provider evidence and consensus.
///
/// This is a recommendation only. It does not modify the
/// DIASISS library.
/// </summary>
public sealed class MetadataRecommendationResult
{
    /// <summary>
    /// Metadata field being recommended.
    ///
    /// Examples:
    /// Artist, Title, Album, Genre, Year, BPM, Duration.
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Recommended metadata value.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Whether DIASISS considers the recommendation safe enough
    /// to present as a strong recommendation.
    /// </summary>
    public bool IsRecommended { get; init; }

    /// <summary>
    /// Consensus strength supporting the recommendation.
    /// </summary>
    public MetadataConsensusStrength Strength { get; init; }

    /// <summary>
    /// Number of independent providers supporting the value.
    /// </summary>
    public int SupportingProviders { get; init; }

    /// <summary>
    /// Number of providers that supplied a value for this field.
    /// </summary>
    public int ProvidersWithValue { get; init; }

    /// <summary>
    /// Percentage of providers supporting the recommendation.
    /// </summary>
    public double AgreementPercentage { get; init; }

    /// <summary>
    /// Human-readable explanation of why the value was recommended.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}