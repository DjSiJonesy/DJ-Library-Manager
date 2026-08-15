using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents one metadata change that DIASISS recommends applying
/// to a physical library file.
///
/// This model represents a proposed change only. It does not modify
/// the DIASISS library and does not represent a provider search result.
///
/// The Search UI can use this model to display:
///
/// - the metadata currently stored in the library
/// - the metadata recommended by DIASISS
/// - the confidence supporting the recommendation
/// - whether the user has selected the change for confirmation
/// </summary>
public sealed class MetadataChangeRecommendation
{
    // ============================================================
    // Metadata Field
    // ============================================================

    /// <summary>
    /// Metadata field being proposed for change.
    ///
    /// Examples:
    /// Artist, Title, Album, Genre, Year, BPM, Duration.
    /// </summary>
    public string Field { get; init; } = string.Empty;

    // ============================================================
    // Existing Metadata
    // ============================================================

    /// <summary>
    /// Value currently stored in the DIASISS library.
    ///
    /// An empty value indicates that the metadata field is currently
    /// missing.
    /// </summary>
    public string CurrentValue { get; init; } = string.Empty;

    // ============================================================
    // Recommended Metadata
    // ============================================================

    /// <summary>
    /// Metadata value recommended by DIASISS based on independent
    /// provider evidence.
    /// </summary>
    public string RecommendedValue { get; init; } = string.Empty;

    // ============================================================
    // Confidence
    // ============================================================

    /// <summary>
    /// Percentage of providers supporting the recommended value.
    /// </summary>
    public double AgreementPercentage { get; init; }

    /// <summary>
    /// Number of independent providers supporting the recommended
    /// value.
    /// </summary>
    public int SupportingProviders { get; init; }

    /// <summary>
    /// Total number of independent providers that supplied a value
    /// for this field.
    /// </summary>
    public int ProvidersWithValue { get; init; }

    /// <summary>
    /// Strength of the provider consensus.
    /// </summary>
    public MetadataConsensusStrength Strength { get; init; }

    // ============================================================
    // Recommendation
    // ============================================================

    /// <summary>
    /// Indicates whether DIASISS considers this change suitable
    /// to recommend to the user.
    ///
    /// This does NOT mean that the change has been approved.
    /// </summary>
    public bool IsRecommended { get; init; }

    /// <summary>
    /// Indicates whether the user has selected this recommendation
    /// for confirmation.
    ///
    /// Selection is presentation/workflow state and does not itself
    /// modify the library.
    /// </summary>
    public bool IsSelected { get; set; }

    // ============================================================
    // Explanation
    // ============================================================

    /// <summary>
    /// Human-readable explanation of why DIASISS made this
    /// recommendation.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    // ============================================================
    // Convenience
    // ============================================================

    /// <summary>
    /// Indicates whether this recommendation represents an actual
    /// change to the current library metadata.
    /// </summary>
    public bool IsChange =>
        !string.Equals(
            CurrentValue?.Trim(),
            RecommendedValue?.Trim(),
            StringComparison.OrdinalIgnoreCase);
}