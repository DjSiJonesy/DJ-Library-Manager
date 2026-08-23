using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Converts independently calculated field-by-field metadata
/// consensus into metadata change recommendations for the
/// DIASISS Search workflow.
///
/// This service does not search providers, perform candidate
/// matching, calculate field consensus, or modify the DIASISS
/// library.
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
    /// Standard recommendation threshold used by metadata fields
    /// that require percentage-based consensus.
    /// </summary>
    private const double RecommendationThreshold = 75.0;

    /// <summary>
    /// Genre is intentionally more permissive because genre
    /// classification is inherently less precise between providers.
    ///
    /// Two independent providers agreeing is sufficient even when
    /// another provider does not agree.
    /// </summary>
    private const int GenreMinimumSupportingProviders = 2;

    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// Converts field-by-field consensus results into metadata
    /// change recommendations.
    ///
    /// CurrentValue is deliberately left empty because this service
    /// does not know the original library media item. The Search
    /// workflow populates CurrentValue when the recommendation is
    /// associated with the actual track.
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
            HasUsableConsensus(
                consensus);

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
    // Consensus Rules
    // ============================================================

    private static bool HasUsableConsensus(
        MetadataConsensusResult consensus)
    {
        if (string.IsNullOrWhiteSpace(
                consensus.Value))
        {
            return false;
        }

        if (consensus.ProvidersWithValue <= 0)
        {
            return false;
        }

        if (consensus.SupportingProviders <= 0)
        {
            return false;
        }

        if (consensus.Strength is
            MetadataConsensusStrength.NoData or
            MetadataConsensusStrength.Conflict)
        {
            return false;
        }

        // --------------------------------------------------------
        // Genre
        // --------------------------------------------------------
        //
        // Genre is intentionally more permissive than the standard
        // metadata fields.
        //
        // A Genre recommendation is accepted when:
        //
        //   1. At least two providers agree
        //
        // OR
        //
        //   2. Every provider that supplied Genre agrees
        //
        // The second rule is important when only one provider has
        // Genre data.
        //
        // Examples:
        //
        //   1 / 1 = 100%  -> recommend
        //   2 / 2 = 100%  -> recommend
        //   2 / 3 = 66.7% -> recommend
        //   1 / 2 = 50%   -> do not recommend
        // --------------------------------------------------------

        if (string.Equals(
                consensus.Field,
                "Genre",
                StringComparison.OrdinalIgnoreCase))
        {
            var allProvidersAgree =
                consensus.SupportingProviders ==
                consensus.ProvidersWithValue;

            var sufficientProviderSupport =
                consensus.SupportingProviders >=
                GenreMinimumSupportingProviders;

            return
                allProvidersAgree ||
                sufficientProviderSupport;
        }

        // --------------------------------------------------------
        // Standard fields
        // --------------------------------------------------------

        return
            consensus.AgreementPercentage >=
            RecommendationThreshold;
    }

    // ============================================================
    // Supported Fields
    // ============================================================

    private static bool IsSupportedField(
        string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return false;
        }

        return
            field.Equals(
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
                "Key",
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

        if (isRecommended &&
            string.Equals(
                consensus.Field,
                "Genre",
                StringComparison.OrdinalIgnoreCase))
        {
            var allProvidersAgree =
                consensus.SupportingProviders ==
                consensus.ProvidersWithValue;

            if (allProvidersAgree)
            {
                return
                    $"{consensus.SupportingProviders} of " +
                    $"{consensus.ProvidersWithValue} providers agree " +
                    $"({consensus.AgreementPercentage:0.#}%). " +
                    "All providers supplying Genre agree.";
            }

            return
                $"{consensus.SupportingProviders} of " +
                $"{consensus.ProvidersWithValue} providers agree " +
                $"({consensus.AgreementPercentage:0.#}%). " +
                $"Genre meets the minimum requirement of " +
                $"{GenreMinimumSupportingProviders} agreeing providers.";
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