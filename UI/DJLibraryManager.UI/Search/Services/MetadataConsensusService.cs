using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Analyses independently validated provider candidates and
/// determines the level of consensus for each metadata field.
///
/// Each provider has one independent vote for a metadata value.
/// Multiple candidates returned by the same provider therefore
/// cannot give that provider additional influence.
///
/// Providers are never compared by passing information from one
/// provider into another. Each provider's independently validated
/// evidence is considered together only at this analysis stage.
///
/// This service does not modify the DIASISS library and does not
/// decide whether a candidate is the correct recording. That has
/// already been handled by MetadataCandidateMatcher.
/// </summary>
public sealed class MetadataConsensusService
{
    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// Analyses independently validated provider candidates and
    /// returns consensus results for the metadata fields currently
    /// supported by DIASISS.
    /// </summary>
    public IReadOnlyList<MetadataConsensusResult> Analyse(
        IEnumerable<MetadataEvidenceAnalysisResult> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var viableCandidates =
            candidates
                .Where(
                    x =>
                        x is not null &&
                        x.Match is not null &&
                        x.Match.IsMatch &&
                        x.Evidence is not null)
                .ToList();

        return new[]
        {
            AnalyseTextField(
                "Artist",
                viableCandidates,
                x => x.Evidence.Artist),

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

            AnalyseBPMField(
                viableCandidates),

            AnalyseDurationField(
                viableCandidates)
        };
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
            GetProviderValues(
                candidates,
                selector,
                NormaliseText);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult(field);
        }

        var groups =
            providerValues
                .GroupBy(
                    x => x.NormalisedValue,
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(
                    x => x.Count())
                .ThenBy(
                    x => x.Key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var highestCount =
            groups[0].Count();

        var winningGroups =
            groups
                .Where(
                    x => x.Count() == highestCount)
                .ToList();

        // --------------------------------------------------------
        // A tie is a genuine conflict.
        //
        // We deliberately do NOT use groups[0] when two different
        // values have equal provider support.
        // --------------------------------------------------------

        if (winningGroups.Count > 1)
        {
            return CreateConflictResult(
                field,
                providerValues);
        }

        var winningGroup =
            winningGroups[0];

        var supporting =
            winningGroup
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflicting =
            providerValues
                .Where(
                    x =>
                        !string.Equals(
                            x.NormalisedValue,
                            winningGroup.Key,
                            StringComparison.OrdinalIgnoreCase))
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            (double)supporting.Count /
            providerValues.Count *
            100;

        return new MetadataConsensusResult
        {
            Field =
                field,

            Value =
                winningGroup
                    .OrderBy(
                        x => x.OriginalValue,
                        StringComparer.OrdinalIgnoreCase)
                    .First()
                    .OriginalValue,

            SupportingProviders =
                supporting.Count,

            ProvidersWithValue =
                providerValues.Count,

            AgreementPercentage =
                Math.Round(
                    agreement,
                    1),

            Strength =
                DetermineStrength(
                    supporting.Count,
                    providerValues.Count,
                    agreement),

            SupportingSources =
                supporting,

            ConflictingSources =
                conflicting
        };
    }

    // ============================================================
    // Year
    // ============================================================

    private static MetadataConsensusResult AnalyseYearField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetProviderValues(
                candidates,
                x =>
                    x.Evidence.Year?.ToString(),
                NormaliseText);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult("Year");
        }

        var groups =
            providerValues
                .GroupBy(
                    x => x.NormalisedValue,
                    StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(
                    x => x.Count())
                .ThenBy(
                    x => x.Key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var highestCount =
            groups[0].Count();

        var winningGroups =
            groups
                .Where(
                    x => x.Count() == highestCount)
                .ToList();

        if (winningGroups.Count > 1)
        {
            return CreateConflictResult(
                "Year",
                providerValues);
        }

        var winningGroup =
            winningGroups[0];

        var supporting =
            winningGroup
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflicting =
            providerValues
                .Where(
                    x =>
                        !string.Equals(
                            x.NormalisedValue,
                            winningGroup.Key,
                            StringComparison.OrdinalIgnoreCase))
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            (double)supporting.Count /
            providerValues.Count *
            100;

        return new MetadataConsensusResult
        {
            Field =
                "Year",

            Value =
                winningGroup.Key,

            SupportingProviders =
                supporting.Count,

            ProvidersWithValue =
                providerValues.Count,

            AgreementPercentage =
                Math.Round(
                    agreement,
                    1),

            Strength =
                DetermineStrength(
                    supporting.Count,
                    providerValues.Count,
                    agreement),

            SupportingSources =
                supporting,

            ConflictingSources =
                conflicting
        };
    }

    // ============================================================
    // BPM
    // ============================================================

    private static MetadataConsensusResult AnalyseBPMField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetNumericProviderValues(
                candidates,
                x => x.Evidence.BPM);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult("BPM");
        }

        var clusters =
            BuildBPMClusters(
                providerValues);

        var largestClusterSize =
            clusters.Max(
                x => x.Count);

        var winningClusters =
            clusters
                .Where(
                    x => x.Count == largestClusterSize)
                .ToList();

        // --------------------------------------------------------
        // Equal support for different BPM groups is a conflict.
        // --------------------------------------------------------

        if (winningClusters.Count > 1)
        {
            return CreateConflictResult(
                "BPM",
                providerValues
                    .Select(
                        x =>
                            new ProviderValue
                            {
                                Provider =
                                    x.Provider,

                                OriginalValue =
                                    x.Value
                                        .ToString(
                                            "0.###"),

                                NormalisedValue =
                                    x.Value
                                        .ToString(
                                            "0.###")
                            })
                    .ToList());
        }

        var winningCluster =
            winningClusters[0];

        var supporting =
            winningCluster.Values
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflicting =
            providerValues
                .Where(
                    x =>
                        !winningCluster.Values.Any(
                            winning =>
                                string.Equals(
                                    winning.Provider,
                                    x.Provider,
                                    StringComparison.OrdinalIgnoreCase)))
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            (double)supporting.Count /
            providerValues.Count *
            100;

        var consensusValue =
            winningCluster.Values
                .Average(
                    x => x.Value);

        return new MetadataConsensusResult
        {
            Field =
                "BPM",

            Value =
                Math.Round(
                    consensusValue,
                    3)
                .ToString("0.###"),

            SupportingProviders =
                supporting.Count,

            ProvidersWithValue =
                providerValues.Count,

            AgreementPercentage =
                Math.Round(
                    agreement,
                    1),

            Strength =
                DetermineStrength(
                    supporting.Count,
                    providerValues.Count,
                    agreement),

            SupportingSources =
                supporting,

            ConflictingSources =
                conflicting
        };
    }

    // ============================================================
    // Duration
    // ============================================================

    private static MetadataConsensusResult AnalyseDurationField(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var providerValues =
            GetDurationProviderValues(
                candidates);

        if (providerValues.Count == 0)
        {
            return CreateNoDataResult("Duration");
        }

        var clusters =
            BuildDurationClusters(
                providerValues);

        var largestClusterSize =
            clusters.Max(
                x => x.Count);

        var winningClusters =
            clusters
                .Where(
                    x => x.Count == largestClusterSize)
                .ToList();

        if (winningClusters.Count > 1)
        {
            return CreateConflictResult(
                "Duration",
                providerValues
                    .Select(
                        x =>
                            new ProviderValue
                            {
                                Provider =
                                    x.Provider,

                                OriginalValue =
                                    x.Value.ToString(
                                        @"mm\:ss"),

                                NormalisedValue =
                                    x.Value.TotalSeconds
                                        .ToString("0.###")
                            })
                    .ToList());
        }

        var winningCluster =
            winningClusters[0];

        var supporting =
            winningCluster.Values
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflicting =
            providerValues
                .Where(
                    x =>
                        !winningCluster.Values.Any(
                            winning =>
                                string.Equals(
                                    winning.Provider,
                                    x.Provider,
                                    StringComparison.OrdinalIgnoreCase)))
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var agreement =
            (double)supporting.Count /
            providerValues.Count *
            100;

        var consensusMilliseconds =
            winningCluster.Values
                .Average(
                    x => x.Value.TotalMilliseconds);

        return new MetadataConsensusResult
        {
            Field =
                "Duration",

            Value =
                TimeSpan
                    .FromMilliseconds(
                        consensusMilliseconds)
                    .ToString(
                        @"mm\:ss"),

            SupportingProviders =
                supporting.Count,

            ProvidersWithValue =
                providerValues.Count,

            AgreementPercentage =
                Math.Round(
                    agreement,
                    1),

            Strength =
                DetermineStrength(
                    supporting.Count,
                    providerValues.Count,
                    agreement),

            SupportingSources =
                supporting,

            ConflictingSources =
                conflicting
        };
    }

    // ============================================================
    // Provider Value Extraction
    // ============================================================

    /// <summary>
    /// Gets at most one value from each provider.
    ///
    /// If a provider returns several candidates, the candidate
    /// with the strongest match score is used.
    ///
    /// If the provider has equally strong candidates with
    /// different values, that provider is considered ambiguous
    /// for this field and contributes no vote.
    /// </summary>
    private static List<ProviderValue>
        GetProviderValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
            Func<MetadataEvidenceAnalysisResult, string?> selector,
            Func<string, string> normalise)
    {
        var result =
            new List<ProviderValue>();

        var providerGroups =
            candidates
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.Evidence.Source))
                .GroupBy(
                    x => x.Evidence.Source,
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
                        x =>
                            !string.IsNullOrWhiteSpace(
                                x.Value))
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values
                    .Max(
                        x => x.Candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        x =>
                            x.Candidate.Match.Score ==
                            highestScore)
                    .ToList();

            var distinctValues =
                strongest
                    .Select(
                        x =>
                            normalise(
                                x.Value!))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            // ----------------------------------------------------
            // Provider cannot give a deterministic vote when its
            // strongest candidates disagree.
            // ----------------------------------------------------

            if (distinctValues.Count > 1)
            {
                continue;
            }

            var selected =
                strongest
                    .OrderBy(
                        x => x.Value,
                        StringComparer.OrdinalIgnoreCase)
                    .First();

            result.Add(
                new ProviderValue
                {
                    Provider =
                        providerGroup.Key,

                    OriginalValue =
                        selected.Value!,

                    NormalisedValue =
                        normalise(
                            selected.Value!)
                });
        }

        return result
            .OrderBy(
                x => x.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<NumericProviderValue>
        GetNumericProviderValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
            Func<MetadataEvidenceAnalysisResult, double?> selector)
    {
        var result =
            new List<NumericProviderValue>();

        var providerGroups =
            candidates
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.Evidence.Source))
                .GroupBy(
                    x => x.Evidence.Source,
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
                        x =>
                            x.Value.HasValue &&
                            x.Value.Value > 0)
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values
                    .Max(
                        x => x.Candidate.Match.Score);

            var strongest =
                values
                    .Where(
                        x =>
                            x.Candidate.Match.Score ==
                            highestScore)
                    .Select(
                        x => x.Value!.Value)
                    .Distinct()
                    .ToList();

            if (strongest.Count != 1)
            {
                continue;
            }

            result.Add(
                new NumericProviderValue
                {
                    Provider =
                        providerGroup.Key,

                    Value =
                        strongest[0]
                });
        }

        return result
            .OrderBy(
                x => x.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<DurationProviderValue>
        GetDurationProviderValues(
            IReadOnlyList<MetadataEvidenceAnalysisResult> candidates)
    {
        var result =
            new List<DurationProviderValue>();

        var providerGroups =
            candidates
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.Evidence.Source))
                .GroupBy(
                    x => x.Evidence.Source,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var providerGroup in providerGroups)
        {
            var values =
                providerGroup
                    .Where(
                        x =>
                            x.Evidence.Duration.HasValue &&
                            x.Evidence.Duration.Value
                                .TotalSeconds > 0)
                    .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            var highestScore =
                values
                    .Max(
                        x => x.Match.Score);

            var strongest =
                values
                    .Where(
                        x =>
                            x.Match.Score ==
                            highestScore)
                    .Select(
                        x => x.Evidence.Duration!.Value)
                    .Distinct()
                    .ToList();

            if (strongest.Count != 1)
            {
                continue;
            }

            result.Add(
                new DurationProviderValue
                {
                    Provider =
                        providerGroup.Key,

                    Value =
                        strongest[0]
                });
        }

        return result
            .OrderBy(
                x => x.Provider,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // BPM Clustering
    // ============================================================

    private static List<BPMCluster>
        BuildBPMClusters(
            IReadOnlyList<NumericProviderValue> values)
    {
        var clusters =
            new List<BPMCluster>();

        foreach (var value in values)
        {
            var matchingClusters =
                clusters
                    .Where(
                        cluster =>
                            cluster.Values.Any(
                                existing =>
                                    BPMValuesEquivalent(
                                        existing.Value,
                                        value.Value)))
                    .ToList();

            if (matchingClusters.Count == 0)
            {
                clusters.Add(
                    new BPMCluster
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

            foreach (var additional in
                     matchingClusters.Skip(1).ToList())
            {
                foreach (var item in additional.Values)
                {
                    target.Values.Add(item);
                }

                clusters.Remove(
                    additional);
            }
        }

        return clusters
            .Select(
                cluster =>
                    new BPMCluster
                    {
                        Values =
                            cluster.Values
                                .GroupBy(
                                    x => x.Provider,
                                    StringComparer.OrdinalIgnoreCase)
                                .Select(
                                    x => x.First())
                                .ToList()
                    })
            .ToList();
    }

    // ============================================================
    // Duration Clustering
    // ============================================================

    private static List<DurationCluster>
        BuildDurationClusters(
            IReadOnlyList<DurationProviderValue> values)
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

            foreach (var additional in
                     matchingClusters.Skip(1).ToList())
            {
                foreach (var item in additional.Values)
                {
                    target.Values.Add(item);
                }

                clusters.Remove(
                    additional);
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
                                    x => x.Provider,
                                    StringComparer.OrdinalIgnoreCase)
                                .Select(
                                    x => x.First())
                                .ToList()
                    })
            .ToList();
    }

    // ============================================================
    // Conflict
    // ============================================================

    private static MetadataConsensusResult CreateConflictResult(
        string field,
        IReadOnlyList<ProviderValue> values)
    {
        var sources =
            values
                .Select(
                    x => x.Provider)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
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
                values.Count,

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
    // No Data
    // ============================================================

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
    // Comparison
    // ============================================================

    private static bool BPMValuesEquivalent(
        double left,
        double right)
    {
        var direct =
            Math.Abs(
                left -
                right);

        if (direct <= 1)
        {
            return true;
        }

        var half =
            right / 2.0;

        var doubled =
            right * 2.0;

        return
            Math.Abs(left - half) <= 1 ||
            Math.Abs(left - doubled) <= 1;
    }

    private static bool DurationsEquivalent(
        TimeSpan left,
        TimeSpan right)
    {
        var difference =
            Math.Abs(
                (
                    left -
                    right)
                .TotalSeconds);

        return difference <= 3;
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

    private sealed class ProviderValue
    {
        public string Provider { get; init; } = string.Empty;

        public string OriginalValue { get; init; } = string.Empty;

        public string NormalisedValue { get; init; } = string.Empty;
    }

    private sealed class NumericProviderValue
    {
        public string Provider { get; init; } = string.Empty;

        public double Value { get; init; }
    }

    private sealed class DurationProviderValue
    {
        public string Provider { get; init; } = string.Empty;

        public TimeSpan Value { get; init; }
    }

    private sealed class BPMCluster
    {
        public List<NumericProviderValue> Values { get; init; } = [];
        public int Count => Values.Count;
    }

    private sealed class DurationCluster
    {
        public List<DurationProviderValue> Values { get; init; } = [];
        public int Count => Values.Count;
    }
}