using System;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Combines a piece of provider metadata evidence with the
/// DIASISS assessment of how well that evidence matches the
/// local library track.
/// </summary>
public sealed class MetadataEvidenceAnalysisResult
{
    /// <summary>
    /// The original provider evidence.
    /// </summary>
    public MetadataEvidence Evidence { get; init; } = null!;

    /// <summary>
    /// DIASISS's candidate match assessment.
    /// </summary>
    public MetadataCandidateMatch Match { get; init; } = null!;
}