using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Analyses independently validated provider evidence
/// field-by-field and determines the strongest consensus
/// for each metadata field.
///
/// Each provider has at most one vote for each metadata field.
/// Missing provider data is not treated as disagreement.
///
/// Artist identity receives specialised handling because
/// providers may legitimately represent the same recording using
/// different levels of artist detail, for example:
///
///     Luude
///     Luude, Colin Hay
///
/// In those cases the more complete compatible identity is preferred
/// rather than treating the values as a conflict.
///
/// This service does not modify the DIASISS library and does not
/// decide whether a metadata change should be applied.
///
/// Recommendation policy is handled separately by
/// MetadataRecommendationService.
/// </summary>
public sealed class MetadataConsensusService
{
    // ============================================================
    // Configuration
    // ============================================================

    /// <summary>
    /// BPM values within this tolerance are considered equivalent.
    /// </summary>
    private const double BpmTolerance = 1.0;

    /// <summary>
    /// Duration values within this tolerance are considered
    /// equivalent.
    /// </summary>
    private const double DurationToleranceSeconds = 3.0;

    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// Analyses independently validated provider evidence
    /// independently for each supported metadata field.
    ///
    /// A provider may contribute at most one vote to each field.
    ///
    /// Providers that do not supply a value for a field are
    /// excluded from that field's evidence pool and are not
    /// considered to disagree.
    /// </summary>
    public IReadOnlyList<MetadataConsensusResult> Analyse(
        IEnumerable<MetadataEvidenceAnalysisResult> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var viableCandidates =
            candidates
                .Where(
                    candidate =>
                        candidate is not null &&
                        candidate.Match is not null &&
                        candidate.Match.IsMatch &&
                        candidate.Evidence is not null)
                .ToList();

        return new[]
        {
            AnalyseArtistField(
                viableCandidates),

            AnalyseTextField(
                "Title",
                viableCandidates,
                x => x.Evidence.Title),

            AnalyseTextField(
                "Album",
                viableCandidates,
                x => x.Evidence.Album),

            AnalyseTextField(
                "Genre",
                viableCandidates,
                x => x.Evidence.Genre),

            AnalyseYearField(
                viableCandidates),

            AnalyseBpmField(
                viableCandidates),

            AnalyseKeyField(
                viableCandidates),

            AnalyseDurationField(
                viableCandidates)
        };
    }

    // ============================================================
    // Artist
    // ============================================================

    /// <summary>
    /// Analyses Artist identity using compatibility rather than
    /// strict string equality.
    ///
    /// Providers may legitimately return different levels of
    /// artist detail for the same recording.
    ///
    /// Example:
    ///
    ///     Provider A -> Luude
    ///     Provider B -> Luude, Colin Hay
    ///
    /// These values are considered compatible because the shorter
    /// identity is contained within the more complete identity.
    ///
    /// When compatible identities are found, the most complete
    /// identity is selected.
    ///
    /// Genuine unrelated artist identities still produce a
    /// conflict.
    /// </summary>
    private static MetadataConsensusResult AnalyseArtistField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetProviderTextValues(
                candidates,
                candidate =>
                    candidate.Evidence.Artist);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(
                "Artist");
        }

        var clusters =
            BuildArtistClusters(
                providerValues);

        if (clusters.Count == 0)
        {
            return CreateNoDataResult(
                "Artist");
        }

        var largestClusterSize =
            clusters.Max(
                cluster =>
                    cluster.Values.Count);

        var winningClusters =
            clusters
                .Where(
                    cluster =>
                        cluster.Values.Count ==
                        largestClusterSize)
                .ToList();

        // --------------------------------------------------------
        // If equally supported artist identities are genuinely
        // incompatible, this remains a conflict.
        // --------------------------------------------------------

        if (winningClusters.Count > 1)
        {
            return CreateConflictResult(
                "Artist",
                providerValues
                    .Select(
                        value =>
                            new ProviderValue
                            {
                                Provider =
                                    value.Provider,

                                OriginalValue =
                                    value.OriginalValue,

                                NormalisedValue =
                                    value.NormalisedValue
                            })
                    .ToList());
        }

        var winningCluster =
            winningClusters[0];

        var supportingProviders =
            winningCluster.Values
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providerValues
                .Where(
                    value =>
                        !winningCluster.Values.Contains(
                            value))
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            CalculateAgreement(
                supportingProviders.Count,
                providerValues.Count);

        // --------------------------------------------------------
        // Select the most complete identity in the winning cluster.
        //
        // Do this explicitly rather than using chained LINQ
        // ordering so that the compiler does not have to infer
        // multiple delegate types.
        // --------------------------------------------------------

        var selectedValue =
            winningCluster
                .Values[0]
                .OriginalValue;

        foreach (var candidate in winningCluster.Values)
        {
            var selectedComponentCount =
                GetArtistComponentCount(
                    selectedValue);

            var candidateComponentCount =
                GetArtistComponentCount(
                    candidate.OriginalValue);

            if (candidateComponentCount >
                selectedComponentCount)
            {
                selectedValue =
                    candidate.OriginalValue;

                continue;
            }

            if (candidateComponentCount ==
                selectedComponentCount &&
                candidate.OriginalValue.Length >
                selectedValue.Length)
            {
                selectedValue =
                    candidate.OriginalValue;
            }
        }

        return CreateConsensusResult(
            "Artist",
            selectedValue,
            supportingProviders,
            conflictingProviders,
            agreement);
    }

    /// <summary>
    /// Groups provider Artist values into compatible identities.
    ///
    /// Artist identities are compatible when one identity contains
    /// all of the artist components of the other identity.
    ///
    /// This allows:
    ///
    ///     Luude
    ///
    /// and:
    ///
    ///     Luude, Colin Hay
    ///
    /// to represent the same artist identity while still treating
    /// genuinely unrelated artists as conflicting evidence.
    /// </summary>
    private static List<ArtistCluster>
        BuildArtistClusters(
            IReadOnlyList<ProviderTextValue> values)
    {
        var clusters =
            new List<ArtistCluster>();

        foreach (var value in values)
        {
            var matchingClusters =
                clusters
                    .Where(
                        cluster =>
                            cluster.Values.Any(
                                existing =>
                                    AreArtistIdentitiesCompatible(
                                        existing.OriginalValue,
                                        value.OriginalValue)))
                    .ToList();

            if (matchingClusters.Count == 0)
            {
                clusters.Add(
                    new ArtistCluster
                    {
                        Values =
                        {
                            value
                        }
                    });

                continue;
            }

            var target =
                matchingClusters[0];

            target.Values.Add(
                value);

            foreach (
                var additionalCluster
                in matchingClusters.Skip(1).ToList())
            {
                foreach (
                    var item
                    in additionalCluster.Values)
                {
                    target.Values.Add(
                        item);
                }

                clusters.Remove(
                    additionalCluster);
            }
        }

        return clusters
            .Select(
                cluster =>
                    new ArtistCluster
                    {
                        Values =
                            cluster.Values
                                .GroupBy(
                                    value =>
                                        value.Provider,
                                    StringComparer.OrdinalIgnoreCase)
                                .Select(
                                    group =>
                                        group.First())
                                .ToList()
                    })
            .ToList();
    }

    /// <summary>
    /// Determines whether two Artist identities are compatible.
    ///
    /// The comparison is token based rather than substring based,
    /// preventing values such as "Luude" and "Luudex" from being
    /// incorrectly treated as the same artist.
    /// </summary>
    private static bool AreArtistIdentitiesCompatible(
        string left,
        string right)
    {
        var leftComponents =
            GetArtistComponents(
                left);

        var rightComponents =
            GetArtistComponents(
                right);

        if (leftComponents.Count == 0 ||
            rightComponents.Count == 0)
        {
            return false;
        }

        return
            leftComponents.IsSubsetOf(
                rightComponents) ||
            rightComponents.IsSubsetOf(
                leftComponents);
    }

    /// <summary>
    /// Breaks an Artist identity into normalised artist components.
    ///
    /// Common provider separators are treated as component
    /// boundaries.
    /// </summary>
    private static HashSet<string> GetArtistComponents(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
        }

        var normalised =
            value
                .Trim()
                .ToUpperInvariant();

        var separators =
            new[]
            {
                ",",
                " & ",
                " AND ",
                " FEAT. ",
                " FEAT ",
                " FT. ",
                " FT ",
                " WITH ",
                " VS. ",
                " VS "
            };

        foreach (var separator in separators)
        {
            normalised =
                normalised.Replace(
                    separator,
                    "|",
                    StringComparison.OrdinalIgnoreCase);
        }

        return
            normalised
                .Split(
                    '|',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Select(
                    component =>
                        NormaliseText(
                            component))
                .Where(
                    component =>
                        !string.IsNullOrWhiteSpace(
                            component))
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);
    }

    private static int GetArtistComponentCount(
        string value)
    {
        return GetArtistComponents(value).Count;
    }

    // ============================================================
    // Text Fields
    // ============================================================

    private static MetadataConsensusResult AnalyseTextField(
        string field,
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
        Func<MetadataEvidenceAnalysisResult, string?> selector)
    {
        var providerValues =
            GetProviderTextValues(
                candidates,
                selector);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(
                field);
        }

        var groups =
            providerValues
                .GroupBy(
                    value =>
                        value.NormalisedValue,
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(
                    group =>
                        group.Count())
                .ThenBy(
                    group =>
                        group.Key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var highestCount =
            groups[0].Count();

        var winningGroups =
            groups
                .Where(
                    group =>
                        group.Count() ==
                        highestCount)
                .ToList();

        if (winningGroups.Count > 1)
        {
            return CreateConflictResult(
                field,
                providerValues
                    .Select(
                        value =>
                            new ProviderValue
                            {
                                Provider =
                                    value.Provider,

                                OriginalValue =
                                    value.OriginalValue,

                                NormalisedValue =
                                    value.NormalisedValue
                            })
                    .ToList());
        }

        var winningGroup =
            winningGroups[0];

        var supportingProviders =
            winningGroup
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providerValues
                .Where(
                    value =>
                        !string.Equals(
                            value.NormalisedValue,
                            winningGroup.Key,
                            StringComparison.OrdinalIgnoreCase))
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            CalculateAgreement(
                supportingProviders.Count,
                providerValues.Count);

        var selectedValue =
            winningGroup
                .OrderBy(
                    value =>
                        value.OriginalValue,
                    StringComparer.OrdinalIgnoreCase)
                .First()
                .OriginalValue;

        return CreateConsensusResult(
            field,
            selectedValue,
            supportingProviders,
            conflictingProviders,
            agreement);
    }

    // ============================================================
    // Year
    // ============================================================

    private static MetadataConsensusResult AnalyseYearField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetProviderYearValues(
                candidates);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(
                "Year");
        }

        var yearGroups =
            providerValues
                .GroupBy(
                    value =>
                        value.Year)
                .OrderByDescending(
                    group =>
                        group.Count())
                .ThenBy(
                    group =>
                        group.Key)
                .ToList();

        var highestCount =
            yearGroups[0].Count();

        var winningGroups =
            yearGroups
                .Where(
                    group =>
                        group.Count() ==
                        highestCount)
                .ToList();

        if (winningGroups.Count > 1)
        {
            return CreateConflictResult(
                "Year",
                providerValues
                    .Select(
                        value =>
                            new ProviderValue
                            {
                                Provider =
                                    value.Provider,

                                OriginalValue =
                                    value.Year.ToString(),

                                NormalisedValue =
                                    value.Year.ToString()
                            })
                    .ToList());
        }

        var winningGroup =
            winningGroups[0];

        var selectedYear =
            winningGroup.Key;

        var supportingProviders =
            winningGroup
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providerValues
                .Where(
                    value =>
                        value.Year !=
                        selectedYear)
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            CalculateAgreement(
                supportingProviders.Count,
                providerValues.Count);

        return CreateConsensusResult(
            "Year",
            selectedYear.ToString(),
            supportingProviders,
            conflictingProviders,
            agreement);
    }

    // ============================================================
    // Provider Year Values
    // ============================================================

    private static List<ProviderYearValue>
        GetProviderYearValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var result =
            new List<ProviderYearValue>();

        var providerGroups =
            candidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source))
                .GroupBy(
                    candidate =>
                        candidate.Evidence.Source,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in providerGroups)
        {
            var values =
                providerGroup
                    .Where(
                        candidate =>
                            candidate.Evidence.Year.HasValue &&
                            candidate.Evidence.Year.Value > 0)
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values.Max(
                    candidate =>
                        candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        candidate =>
                            candidate.Match.Score ==
                            highestScore)
                    .Select(
                        candidate =>
                            candidate.Evidence.Year!.Value)
                    .Distinct()
                    .OrderBy(
                        year =>
                            year)
                    .ToList();

            if (strongest.Count == 0)
            {
                continue;
            }

            result.Add(
                new ProviderYearValue
                {
                    Provider =
                        providerGroup.Key,

                    Year =
                        strongest[0]
                });
        }

        return result
            .OrderBy(
                value =>
                    value.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // Key
    // ============================================================

    private static MetadataConsensusResult AnalyseKeyField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        return AnalyseTextField(
            "Key",
            candidates,
            candidate =>
                candidate.Evidence.Key);
    }

    // ============================================================
    // BPM
    // ============================================================

    private static MetadataConsensusResult AnalyseBpmField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetProviderNumericValues(
                candidates,
                candidate =>
                    candidate.Evidence.BPM);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(
                "BPM");
        }

        var clusters =
            BuildBpmClusters(
                providerValues);

        var largestClusterSize =
            clusters.Max(
                cluster =>
                    cluster.Count);

        var winningClusters =
            clusters
                .Where(
                    cluster =>
                        cluster.Count ==
                        largestClusterSize)
                .ToList();

        if (winningClusters.Count > 1)
        {
            return CreateConflictResult(
                "BPM",
                providerValues
                    .Select(
                        value =>
                            new ProviderValue
                            {
                                Provider =
                                    value.Provider,

                                OriginalValue =
                                    value.Value.ToString(
                                        "0.###"),

                                NormalisedValue =
                                    value.Value.ToString(
                                        "0.###")
                            })
                    .ToList());
        }

        var winningCluster =
            winningClusters[0];

        var supportingProviders =
            winningCluster.Values
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providerValues
                .Where(
                    value =>
                        !winningCluster.Values.Any(
                            winning =>
                                string.Equals(
                                    winning.Provider,
                                    value.Provider,
                                    StringComparison.OrdinalIgnoreCase)))
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            CalculateAgreement(
                supportingProviders.Count,
                providerValues.Count);

        var consensusValue =
            winningCluster.Values
                .Average(
                    value =>
                        value.Value);

        return CreateConsensusResult(
            "BPM",
            Math.Round(
                    consensusValue,
                    3)
                .ToString(
                    "0.###"),
            supportingProviders,
            conflictingProviders,
            agreement);
    }

    // ============================================================
    // Duration
    // ============================================================

    private static MetadataConsensusResult AnalyseDurationField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetProviderDurationValues(
                candidates);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(
                "Duration");
        }

        var clusters =
            BuildDurationClusters(
                providerValues);

        var largestClusterSize =
            clusters.Max(
                cluster =>
                    cluster.Count);

        var winningClusters =
            clusters
                .Where(
                    cluster =>
                        cluster.Count ==
                        largestClusterSize)
                .ToList();

        if (winningClusters.Count > 1)
        {
            return CreateConflictResult(
                "Duration",
                providerValues
                    .Select(
                        value =>
                            new ProviderValue
                            {
                                Provider =
                                    value.Provider,

                                OriginalValue =
                                    value.Value.ToString(
                                        @"mm\:ss"),

                                NormalisedValue =
                                    value.Value.TotalSeconds
                                        .ToString(
                                            "0.###")
                            })
                    .ToList());
        }

        var winningCluster =
            winningClusters[0];

        var supportingProviders =
            winningCluster.Values
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providerValues
                .Where(
                    value =>
                        !winningCluster.Values.Any(
                            winning =>
                                string.Equals(
                                    winning.Provider,
                                    value.Provider,
                                    StringComparison.OrdinalIgnoreCase)))
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            CalculateAgreement(
                supportingProviders.Count,
                providerValues.Count);

        var consensusMilliseconds =
            winningCluster.Values
                .Average(
                    value =>
                        value.Value.TotalMilliseconds);

        var consensusDuration =
            TimeSpan.FromMilliseconds(
                consensusMilliseconds);

        return CreateConsensusResult(
            "Duration",
            consensusDuration.ToString(
                @"mm\:ss"),
            supportingProviders,
            conflictingProviders,
            agreement);
    }

    // ============================================================
    // Provider Text Values
    // ============================================================

    private static List<ProviderTextValue>
        GetProviderTextValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
            Func<MetadataEvidenceAnalysisResult, string?> selector)
    {
        var result =
            new List<ProviderTextValue>();

        var providerGroups =
            candidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source))
                .GroupBy(
                    candidate =>
                        candidate.Evidence.Source,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in providerGroups)
        {
            var values =
                providerGroup
                    .Select(
                        candidate =>
                            new
                            {
                                Candidate =
                                    candidate,

                                Value =
                                    selector(candidate)
                            })
                    .Where(
                        item =>
                            !string.IsNullOrWhiteSpace(
                                item.Value))
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values.Max(
                    item =>
                        item.Candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        item =>
                            item.Candidate.Match.Score ==
                            highestScore)
                    .ToList();

            var distinctValues =
                strongest
                    .Select(
                        item =>
                            NormaliseText(
                                item.Value!))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            if (distinctValues.Count > 1)
            {
                continue;
            }

            var selected =
                strongest
                    .OrderBy(
                        item =>
                            item.Value,
                        StringComparer.OrdinalIgnoreCase)
                    .First();

            result.Add(
                new ProviderTextValue
                {
                    Provider =
                        providerGroup.Key,

                    OriginalValue =
                        selected.Value!,

                    NormalisedValue =
                        NormaliseText(
                            selected.Value!)
                });
        }

        return result
            .OrderBy(
                value =>
                    value.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // Provider Numeric Values
    // ============================================================

    private static List<ProviderNumericValue>
        GetProviderNumericValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
            Func<MetadataEvidenceAnalysisResult, double?> selector)
    {
        var result =
            new List<ProviderNumericValue>();

        var providerGroups =
            candidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source))
                .GroupBy(
                    candidate =>
                        candidate.Evidence.Source,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in providerGroups)
        {
            var values =
                providerGroup
                    .Select(
                        candidate =>
                            new
                            {
                                Candidate =
                                    candidate,

                                Value =
                                    selector(candidate)
                            })
                    .Where(
                        item =>
                            item.Value.HasValue &&
                            item.Value.Value > 0)
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values.Max(
                    item =>
                        item.Candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        item =>
                            item.Candidate.Match.Score ==
                            highestScore)
                    .Select(
                        item =>
                            item.Value!.Value)
                    .Distinct()
                    .ToList();

            if (strongest.Count != 1)
            {
                continue;
            }

            result.Add(
                new ProviderNumericValue
                {
                    Provider =
                        providerGroup.Key,

                    Value =
                        strongest[0]
                });
        }

        return result
            .OrderBy(
                value =>
                    value.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // Provider Duration Values
    // ============================================================

    private static List<ProviderDurationValue>
        GetProviderDurationValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var result =
            new List<ProviderDurationValue>();

        var providerGroups =
            candidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source))
                .GroupBy(
                    candidate =>
                        candidate.Evidence.Source,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in providerGroups)
        {
            var values =
                providerGroup
                    .Where(
                        candidate =>
                            candidate.Evidence.Duration.HasValue &&
                            candidate.Evidence.Duration.Value
                                .TotalSeconds > 0)
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values.Max(
                    candidate =>
                        candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        candidate =>
                            candidate.Match.Score ==
                            highestScore)
                    .Select(
                        candidate =>
                            candidate.Evidence.Duration!.Value)
                    .Distinct()
                    .ToList();

            if (strongest.Count != 1)
            {
                continue;
            }

            result.Add(
                new ProviderDurationValue
                {
                    Provider =
                        providerGroup.Key,

                    Value =
                        strongest[0]
                });
        }

        return result
            .OrderBy(
                value =>
                    value.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // BPM Clustering
    // ============================================================

    private static List<BpmCluster>
        BuildBpmClusters(
            IReadOnlyList<ProviderNumericValue> values)
    {
        var clusters =
            new List<BpmCluster>();

        foreach (var value in values)
        {
            var matchingClusters =
                clusters
                    .Where(
                        cluster =>
                            cluster.Values.Any(
                                existing =>
                                    BpmValuesEquivalent(
                                        existing.Value,
                                        value.Value)))
                    .ToList();

            if (matchingClusters.Count == 0)
            {
                clusters.Add(
                    new BpmCluster
                    {
                        Values =
                        {
                            value
                        }
                    });

                continue;
            }

            var target =
                matchingClusters[0];

            target.Values.Add(
                value);

            foreach (var additionalCluster in
                     matchingClusters.Skip(1).ToList())
            {
                foreach (var item in
                         additionalCluster.Values)
                {
                    target.Values.Add(
                        item);
                }

                clusters.Remove(
                    additionalCluster);
            }
        }

        return clusters
            .Select(
                cluster =>
                    new BpmCluster
                    {
                        Values =
                            cluster.Values
                                .GroupBy(
                                    value =>
                                        value.Provider,
                                    StringComparer.OrdinalIgnoreCase)
                                .Select(
                                    group =>
                                        group.First())
                                .ToList()
                    })
            .ToList();
    }

    // ============================================================
    // Duration Clustering
    // ============================================================

    private static List<DurationCluster>
        BuildDurationClusters(
            IReadOnlyList<ProviderDurationValue> values)
    {
        var clusters =
            new List<DurationCluster>();

        foreach (var value in values)
        {
            var matchingClusters =
                clusters
                    .Where(
                        cluster =>
                            cluster.Values.Any(
                                existing =>
                                    DurationsEquivalent(
                                        existing.Value,
                                        value.Value)))
                    .ToList();

            if (matchingClusters.Count == 0)
            {
                clusters.Add(
                    new DurationCluster
                    {
                        Values =
                        {
                            value
                        }
                    });

                continue;
            }

            var target =
                matchingClusters[0];

            target.Values.Add(
                value);

            foreach (var additionalCluster in
                     matchingClusters.Skip(1).ToList())
            {
                foreach (var item in
                         additionalCluster.Values)
                {
                    target.Values.Add(
                        item);
                }

                clusters.Remove(
                    additionalCluster);
            }
        }

        return clusters
            .Select(
                cluster =>
                    new DurationCluster
                    {
                        Values =
                            cluster.Values
                                .GroupBy(
                                    value =>
                                        value.Provider,
                                    StringComparer.OrdinalIgnoreCase)
                                .Select(
                                    group =>
                                        group.First())
                                .ToList()
                    })
            .ToList();
    }

    // ============================================================
    // Consensus Result
    // ============================================================

    private static MetadataConsensusResult CreateConsensusResult(
        string field,
        string value,
        IReadOnlyList<string> supportingProviders,
        IReadOnlyList<string> conflictingProviders,
        double agreementPercentage)
    {
        return new MetadataConsensusResult
        {
            Field =
                field,

            Value =
                value,

            SupportingProviders =
                supportingProviders.Count,

            ProvidersWithValue =
                supportingProviders.Count +
                conflictingProviders.Count,

            AgreementPercentage =
                Math.Round(
                    agreementPercentage,
                    1),

            Strength =
                DetermineStrength(
                    supportingProviders.Count,
                    supportingProviders.Count +
                    conflictingProviders.Count,
                    agreementPercentage),

            SupportingSources =
                supportingProviders,

            ConflictingSources =
                conflictingProviders
        };
    }

    private static MetadataConsensusResult CreateConflictResult(
        string field,
        IReadOnlyList<ProviderValue> values)
    {
        var sources =
            values
                .Select(
                    value =>
                        value.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                        StringComparer.OrdinalIgnoreCase)
                .ToList();

        return new MetadataConsensusResult
        {
            Field =
                field,

            Value =
                string.Empty,

            SupportingProviders =
                0,

            ProvidersWithValue =
                sources.Count,

            AgreementPercentage =
                0,

            Strength =
                MetadataConsensusStrength.Conflict,

            SupportingSources =
                Array.Empty<string>(),

            ConflictingSources =
                sources
        };
    }

    private static MetadataConsensusResult CreateNoDataResult(
        string field)
    {
        return new MetadataConsensusResult
        {
            Field =
                field,

            Value =
                string.Empty,

            SupportingProviders =
                0,

            ProvidersWithValue =
                0,

            AgreementPercentage =
                0,

            Strength =
                MetadataConsensusStrength.NoData,

            SupportingSources =
                Array.Empty<string>(),

            ConflictingSources =
                Array.Empty<string>()
        };
    }

    // ============================================================
    // Agreement
    // ============================================================

    private static double CalculateAgreement(
        int supportingProviders,
        int providersWithValue)
    {
        if (providersWithValue <= 0)
        {
            return 0;
        }

        return
            (double)supportingProviders /
            providersWithValue *
            100;
    }

    // ============================================================
    // Strength
    // ============================================================

    private static MetadataConsensusStrength DetermineStrength(
        int supportingProviders,
        int providersWithValue,
        double agreementPercentage)
    {
        if (providersWithValue == 0)
        {
            return MetadataConsensusStrength.NoData;
        }

        if (supportingProviders <= 0)
        {
            return MetadataConsensusStrength.Conflict;
        }

        if (agreementPercentage >= 100)
        {
            return MetadataConsensusStrength.Strong;
        }

        if (agreementPercentage >= 75)
        {
            return MetadataConsensusStrength.Moderate;
        }

        if (agreementPercentage >= 50)
        {
            return MetadataConsensusStrength.Weak;
        }

        return MetadataConsensusStrength.Conflict;
    }

    // ============================================================
    // BPM Comparison
    // ============================================================

    private static bool BpmValuesEquivalent(
        double left,
        double right)
    {
        if (Math.Abs(left - right) <= BpmTolerance)
        {
            return true;
        }

        if (Math.Abs(left - (right / 2.0)) <= BpmTolerance)
        {
            return true;
        }

        if (Math.Abs(left - (right * 2.0)) <= BpmTolerance)
        {
            return true;
        }

        return false;
    }

    // ============================================================
    // Duration Comparison
    // ============================================================

    private static bool DurationsEquivalent(
        TimeSpan left,
        TimeSpan right)
    {
        return
            Math.Abs(
                (
                    left -
                    right)
                .TotalSeconds)
            <= DurationToleranceSeconds;
    }

    // ============================================================
    // Text Normalisation
    // ============================================================

    private static string NormaliseText(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            " ",
            value
                .Trim()
                .ToUpperInvariant()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }

    // ============================================================
    // Internal Models
    // ============================================================

    private sealed class ProviderTextValue
    {
        public string Provider { get; init; } = string.Empty;

        public string OriginalValue { get; init; } = string.Empty;

        public string NormalisedValue { get; init; } = string.Empty;
    }

    private sealed class ProviderNumericValue
    {
        public string Provider { get; init; } = string.Empty;

        public double Value { get; init; }
    }

    private sealed class ProviderYearValue
    {
        public string Provider { get; init; } = string.Empty;

        public int Year { get; init; }
    }

    private sealed class ProviderDurationValue
    {
        public string Provider { get; init; } = string.Empty;

        public TimeSpan Value { get; init; }
    }

    private sealed class ProviderValue
    {
        public string Provider { get; init; } = string.Empty;

        public string OriginalValue { get; init; } = string.Empty;

        public string NormalisedValue { get; init; } = string.Empty;
    }

    private sealed class ArtistCluster
    {
        public List<ProviderTextValue> Values { get; init; } = [];
    }

    private sealed class BpmCluster
    {
        public List<ProviderNumericValue> Values { get; init; } = [];

        public int Count =>
            Values.Count;
    }

    private sealed class DurationCluster
    {
        public List<ProviderDurationValue> Values { get; init; } = [];

        public int Count =>
            Values.Count;
    }
}