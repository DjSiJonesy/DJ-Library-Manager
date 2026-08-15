namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents DIASISS's assessment of how closely a metadata
/// candidate matches the local library track.
///
/// This is candidate matching evidence only. It is not the
/// final metadata recommendation and does not modify the
/// DIASISS library.
/// </summary>
public sealed class MetadataCandidateMatch
{
    // ============================================================
    // Overall Match
    // ============================================================

    /// <summary>
    /// Overall candidate match score, expressed as 0-100.
    ///
    /// This represents how closely the external candidate
    /// matches the local track. It is not the provider's own
    /// confidence value.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Indicates whether the candidate is considered a viable
    /// match for further DIASISS analysis.
    /// </summary>
    public bool IsMatch { get; init; }

    // ============================================================
    // Field Scores
    // ============================================================

    /// <summary>
    /// Artist comparison score, expressed as 0-100.
    /// </summary>
    public double ArtistScore { get; init; }

    /// <summary>
    /// Title comparison score, expressed as 0-100.
    /// </summary>
    public double TitleScore { get; init; }

    /// <summary>
    /// Duration comparison score, expressed as 0-100.
    /// </summary>
    public double DurationScore { get; init; }

    /// <summary>
    /// BPM comparison score, expressed as 0-100.
    /// </summary>
    public double BPMScore { get; init; }

    // ============================================================
    // BPM Relationship
    // ============================================================

    /// <summary>
    /// Indicates that the provider BPM and local BPM are
    /// effectively the same tempo when accounting for a
    /// half-time or double-time relationship.
    /// </summary>
    public bool BPMHalfDoubleMatch { get; init; }

    // ============================================================
    // Explanation
    // ============================================================

    /// <summary>
    /// Human-readable explanation of the candidate assessment.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}