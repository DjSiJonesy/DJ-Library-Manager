using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents DIASISS's recommendation for a single metadata
/// field.
///
/// A recommendation is the result of DIASISS analysis of one or
/// more pieces of metadata evidence. It does not modify the
/// library.
/// </summary>
public sealed class MetadataFieldRecommendation
{
    // ============================================================
    // Field
    // ============================================================

    /// <summary>
    /// Name of the metadata field being recommended.
    ///
    /// Examples:
    /// Artist, Title, Year, Genre, Duration, BPM, Key.
    /// </summary>
    public string Field { get; init; } = string.Empty;

    // ============================================================
    // Recommendation
    // ============================================================

    /// <summary>
    /// The value DIASISS recommends for the field.
    ///
    /// The value is represented as text so that the same model
    /// can represent strings, numbers and durations.
    /// </summary>
    public string RecommendedValue { get; init; }
        = string.Empty;

    /// <summary>
    /// DIASISS confidence in the recommended value,
    /// expressed as 0-100.
    /// </summary>
    public double Confidence { get; init; }

    // ============================================================
    // Decision
    // ============================================================

    /// <summary>
    /// Indicates whether DIASISS considers the recommendation
    /// safe to apply automatically.
    /// </summary>
    public bool SafeToApply { get; init; }

    // ============================================================
    // Explanation
    // ============================================================

    /// <summary>
    /// Human-readable explanation of why DIASISS recommends
    /// this value.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}