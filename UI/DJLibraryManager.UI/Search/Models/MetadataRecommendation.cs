using System.Collections.Generic;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents DIASISS's overall metadata recommendation for
/// one library track.
///
/// The recommendation contains independent field-level
/// decisions. It does not modify the DIASISS library.
/// </summary>
public sealed class MetadataRecommendation
{
    // ============================================================
    // Track
    // ============================================================

    /// <summary>
    /// Physical file to which this recommendation relates.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    // ============================================================
    // Field Recommendations
    // ============================================================

    /// <summary>
    /// DIASISS recommendation for the Artist field.
    /// </summary>
    public MetadataFieldRecommendation? Artist { get; init; }

    /// <summary>
    /// DIASISS recommendation for the Title field.
    /// </summary>
    public MetadataFieldRecommendation? Title { get; init; }

    /// <summary>
    /// DIASISS recommendation for the Year field.
    /// </summary>
    public MetadataFieldRecommendation? Year { get; init; }

    /// <summary>
    /// DIASISS recommendation for the Genre field.
    /// </summary>
    public MetadataFieldRecommendation? Genre { get; init; }

    /// <summary>
    /// DIASISS recommendation for the Duration field.
    /// </summary>
    public MetadataFieldRecommendation? Duration { get; init; }

    /// <summary>
    /// DIASISS recommendation for the BPM field.
    /// </summary>
    public MetadataFieldRecommendation? BPM { get; init; }

    /// <summary>
    /// DIASISS recommendation for the Key field.
    ///
    /// Key is optional metadata and should not normally prevent
    /// the remaining metadata from being recommended.
    /// </summary>
    public MetadataFieldRecommendation? Key { get; init; }

    // ============================================================
    // Summary
    // ============================================================

    /// <summary>
    /// Indicates whether DIASISS considers the track's required
    /// metadata sufficiently reliable to be improved.
    /// </summary>
    public bool SafeToImprove { get; init; }

    /// <summary>
    /// Number of metadata fields that DIASISS considers safe
    /// to apply automatically.
    /// </summary>
    public int SafeFieldCount { get; init; }

    /// <summary>
    /// Overall explanation of the recommendation.
    ///</summary>
    public string Summary { get; init; } = string.Empty;

    // ============================================================
    // Evidence
    // ============================================================

    /// <summary>
    /// Evidence considered when producing this recommendation.
    ///
    /// This allows the recommendation to remain explainable and
    /// auditable.
    /// </summary>
    public IReadOnlyList<MetadataEvidence> Evidence { get; init; }
        = [];
}