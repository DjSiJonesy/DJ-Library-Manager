using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Converts independently calculated metadata consensus into
/// metadata change recommendations for the DIASISS Search workflow.
///
/// This service does not search providers, perform candidate
/// matching, or modify the DIASISS library.
///
/// It only determines whether an established consensus represents
/// a useful metadata change recommendation.
/// </summary>
public sealed class MetadataRecommendationService
{
    // ============================================================
    // Configuration
    // ============================================================

    /// <summary>
    /// Consensus must reach at least this percentage before a
    /// metadata value can be recommended.
    ///
    /// This is the DIASISS recommendation threshold.
    ///
    /// The eventual "Confirm All" threshold is a separate
    /// user/workflow decision and should not be confused with this.
    /// </summary>
    private const double RecommendationThreshold = 75.0;

    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// Converts consensus results into metadata change
    /// recommendations.
    ///
    /// This method does not know the original track metadata.
    /// CurrentValue is therefore initially empty and is populated
    /// by the Search workflow when the recommendation is associated
    /// with the actual library track.
    ///
    /// Key is deliberately excluded because DIASISS currently
    /// treats Key as lower-priority metadata.
    /// </summary>
    public IReadOnlyList<MetadataChangeRecommendation> Recommend(
        IEnumerable<MetadataConsensusResult> consensusResults)
    {
        ArgumentNullException.ThrowIfNull(consensusResults);

        return consensusResults
            .Where(
                result =>
                    result is not null &&
                    IsSupportedField(result.Field))
            .Select(
                CreateRecommendation)
            .ToList();
    }

    // ============================================================
    // Recommendation
    // ============================================================

    private static MetadataChangeRecommendation CreateRecommendation(
        MetadataConsensusResult consensus)
    {
        var isRecommended =
            !string.IsNullOrWhiteSpace(
                consensus.Value) &&

            consensus.ProvidersWithValue > 0 &&

            consensus.SupportingProviders > 0 &&

            consensus.AgreementPercentage >=
                RecommendationThreshold &&

            consensus.Strength is
                MetadataConsensusStrength.Strong or
                MetadataConsensusStrength.Moderate;

        return new MetadataChangeRecommendation
        {
            Field =
                consensus.Field,

            CurrentValue =
                string.Empty,

            RecommendedValue =
                isRecommended
                    ? consensus.Value
                    : string.Empty,

            IsRecommended =
                isRecommended,

            IsSelected =
                false,

            Strength =
                consensus.Strength,

            SupportingProviders =
                consensus.SupportingProviders,

            ProvidersWithValue =
                consensus.ProvidersWithValue,

            AgreementPercentage =
                consensus.AgreementPercentage,

            Reason =
                BuildReason(
                    consensus,
                    isRecommended)
        };
    }

    // ============================================================
    // Supported Fields
    // ============================================================

    private static bool IsSupportedField(
        string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return false;

        return field.Equals(
                   "Artist",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "Title",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "Album",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "Genre",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "Year",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "BPM",
                   StringComparison.OrdinalIgnoreCase) ||

               field.Equals(
                   "Duration",
                   StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // Reason
    // ============================================================

    private static string BuildReason(
        MetadataConsensusResult consensus,
        bool isRecommended)
    {
        if (consensus.Strength ==
            MetadataConsensusStrength.NoData)
        {
            return
                "No provider supplied usable evidence for this field.";
        }

        if (consensus.Strength ==
            MetadataConsensusStrength.Conflict)
        {
            return
                "Providers disagree about this field. " +
                "No automatic metadata change is recommended.";
        }

        if (!isRecommended)
        {
            return
                $"Consensus is {consensus.AgreementPercentage:0.#}% " +
                $"but does not meet the recommendation threshold of " +
                $"{RecommendationThreshold:0.#}%.";
        }

        return
            $"{consensus.SupportingProviders} of " +
            $"{consensus.ProvidersWithValue} providers agree " +
            $"({consensus.AgreementPercentage:0.#}%). " +
            $"Consensus strength: {consensus.Strength}.";
    }
}