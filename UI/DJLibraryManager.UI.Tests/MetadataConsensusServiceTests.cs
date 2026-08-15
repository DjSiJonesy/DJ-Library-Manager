using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataConsensusServiceTests
{
    private readonly MetadataConsensusService _service =
        new();

    // ============================================================
    // Artist
    // ============================================================

    [Fact]
    public void Artist_ThreeProvidersAgree_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    artist: "Darude"),

                CreateEvidence(
                    "Discogs",
                    artist: "Darude"),

                CreateEvidence(
                    "ReccoBeats",
                    artist: "Darude"));

        var result =
            GetResult(
                candidates,
                "Artist");

        Assert.Equal(
            "Darude",
            result.Value);

        Assert.Equal(
            3,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.Empty(
            result.ConflictingSources);
    }

    // ============================================================
    // Title
    // ============================================================

    [Fact]
    public void Title_ThreeProvidersAgree_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    title: "Sandstorm"),

                CreateEvidence(
                    "Discogs",
                    title: "Sandstorm"),

                CreateEvidence(
                    "ReccoBeats",
                    title: "Sandstorm"));

        var result =
            GetResult(
                candidates,
                "Title");

        Assert.Equal(
            "Sandstorm",
            result.Value);

        Assert.Equal(
            3,
            result.SupportingProviders);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);
    }

    // ============================================================
    // Year
    // ============================================================

    [Fact]
    public void Year_TwoOfThreeProvidersAgree_ReturnsWeakConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    year: 1999),

                CreateEvidence(
                    "Discogs",
                    year: 1999),

                CreateEvidence(
                    "ReccoBeats",
                    year: 2001));

        var result =
            GetResult(
                candidates,
                "Year");

        Assert.Equal(
            "1999",
            result.Value);

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            66.7,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Weak,
            result.Strength);

        Assert.Contains(
            "ReccoBeats",
            result.ConflictingSources);
    }

    // ============================================================
    // Genre Conflict
    // ============================================================

    [Fact]
    public void Genre_ThreeDifferentValues_ReturnsConflict()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    genre: "Trance"),

                CreateEvidence(
                    "Discogs",
                    genre: "Electronic"),

                CreateEvidence(
                    "ReccoBeats",
                    genre: "Dance"));

        var result =
            GetResult(
                candidates,
                "Genre");

        Assert.Equal(
            0,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            0,
            result.AgreementPercentage);

        Assert.Empty(
            result.Value);

        Assert.Equal(
            MetadataConsensusStrength.Conflict,
            result.Strength);

        Assert.Equal(
            3,
            result.ConflictingSources.Count);

        Assert.Contains(
            "MusicBrainz",
            result.ConflictingSources);

        Assert.Contains(
            "Discogs",
            result.ConflictingSources);

        Assert.Contains(
            "ReccoBeats",
            result.ConflictingSources);
    }

    // ============================================================
    // BPM
    // ============================================================

    [Fact]
    public void BPM_SlightProviderDifferences_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    bpm: 136.000),

                CreateEvidence(
                    "Discogs",
                    bpm: 136.067),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 136.065));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Equal(
            3,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.InRange(
            double.Parse(result.Value),
            136.0,
            136.1);
    }

    // ============================================================
    // BPM Half / Double
    // ============================================================

    [Fact]
    public void BPM_HalfDoubleRelationship_IsRecognisedAsAgreement()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "VirtualDJ",
                    bpm: 86.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 172.0));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);
    }

    // ============================================================
    // Duration
    // ============================================================

    [Fact]
    public void Duration_SmallDifferences_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    duration:
                        TimeSpan.FromSeconds(225)),

                CreateEvidence(
                    "Discogs",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_493)),

                CreateEvidence(
                    "ReccoBeats",
                    duration:
                        TimeSpan.FromMilliseconds(
                            224_900)));

        var result =
            GetResult(
                candidates,
                "Duration");

        Assert.Equal(
            3,
            result.SupportingProviders);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.Equal(
            "03:45",
            result.Value);
    }

    // ============================================================
    // No Data
    // ============================================================

    [Fact]
    public void Genre_NoProvidersSupplyGenre_ReturnsNoData()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz"),

                CreateEvidence(
                    "Discogs"),

                CreateEvidence(
                    "ReccoBeats"));

        var result =
            GetResult(
                candidates,
                "Genre");

        Assert.Equal(
            MetadataConsensusStrength.NoData,
            result.Strength);

        Assert.Equal(
            0,
            result.SupportingProviders);

        Assert.Equal(
            0,
            result.ProvidersWithValue);

        Assert.Empty(
            result.Value);
    }

    // ============================================================
    // Determinism
    // ============================================================

    /// <summary>
    /// Two providers agreeing must produce the same consensus even
    /// when the disagreeing provider appears first in the evidence.
    ///
    /// This specifically protects against provider/result ordering
    /// affecting the selected metadata value.
    /// </summary>
    [Fact]
    public void Year_TwoProvidersAgree_ResultDoesNotDependOnProviderOrder()
    {
        var firstOrder =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    year: 2007),

                CreateEvidence(
                    "Discogs",
                    year: 2000),

                CreateEvidence(
                    "ReccoBeats",
                    year: 2000));

        var secondOrder =
            CreateCandidates(
                CreateEvidence(
                    "ReccoBeats",
                    year: 2000),

                CreateEvidence(
                    "MusicBrainz",
                    year: 2007),

                CreateEvidence(
                    "Discogs",
                    year: 2000));

        var firstResult =
            GetResult(
                firstOrder,
                "Year");

        var secondResult =
            GetResult(
                secondOrder,
                "Year");

        Assert.Equal(
            firstResult.Value,
            secondResult.Value);

        Assert.Equal(
            "2000",
            firstResult.Value);

        Assert.Equal(
            2,
            firstResult.SupportingProviders);

        Assert.Equal(
            2,
            secondResult.SupportingProviders);

        Assert.Equal(
            firstResult.AgreementPercentage,
            secondResult.AgreementPercentage);

        Assert.Equal(
            firstResult.Strength,
            secondResult.Strength);
    }

    /// <summary>
    /// A provider returning multiple candidates must not receive
    /// multiple votes.
    ///
    /// The provider with two candidates represents one independent
    /// source, not two independent sources.
    /// </summary>
   [Fact]
    public void Genre_MultipleCandidatesWithSameProviderValue_CountsProviderOnlyOnce()
    {
        var candidates =
            CreateCandidates(
                // ReccoBeats returns two candidates, but both agree
                // on the same Genre. This is still one provider vote.
                CreateEvidence(
                    "ReccoBeats",
                    genre: "Trance"),

                CreateEvidence(
                    "ReccoBeats",
                    genre: "Trance"),

                CreateEvidence(
                    "MusicBrainz",
                    genre: "Trance"),

                CreateEvidence(
                    "Discogs",
                    genre: "Dance"));

        var result =
            GetResult(
                candidates,
                "Genre");

        // ReccoBeats contributes ONE vote, not two.
        Assert.Equal(
            2,
            result.SupportingProviders);

        // There are three independent providers.
        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            "Trance",
            result.Value);

        Assert.Equal(
            66.7,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Weak,
            result.Strength);

        Assert.Contains(
            "ReccoBeats",
            result.SupportingSources);

        Assert.Contains(
            "MusicBrainz",
            result.SupportingSources);

        Assert.Contains(
            "Discogs",
            result.ConflictingSources);
    }

    /// <summary>
    /// If the strongest candidates returned by the same provider
    /// disagree, that provider cannot provide a deterministic vote.
    ///
    /// This prevents an arbitrary candidate from being selected
    /// simply because it appeared first.
    /// </summary>
    [Fact]
    public void Genre_SameProviderStrongCandidatesConflict_ProviderDoesNotVote()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "ReccoBeats",
                    genre: "Trance"),

                CreateEvidence(
                    "ReccoBeats",
                    genre: "Electronic"),

                CreateEvidence(
                    "MusicBrainz",
                    genre: "Trance"),

                CreateEvidence(
                    "Discogs",
                    genre: "Trance"));

        var result =
            GetResult(
                candidates,
                "Genre");

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            "Trance",
            result.Value);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.DoesNotContain(
            "ReccoBeats",
            result.SupportingSources);
    }

    /// <summary>
    /// A complete three-way tie must never select an arbitrary
    /// provider's value.
    /// </summary>
    [Fact]
    public void Year_ThreeDifferentValues_NeverSelectsArbitraryWinner()
    {
        var firstOrder =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    year: 1999),

                CreateEvidence(
                    "Discogs",
                    year: 2000),

                CreateEvidence(
                    "ReccoBeats",
                    year: 2001));

        var secondOrder =
            CreateCandidates(
                CreateEvidence(
                    "ReccoBeats",
                    year: 2001),

                CreateEvidence(
                    "MusicBrainz",
                    year: 1999),

                CreateEvidence(
                    "Discogs",
                    year: 2000));

        var firstResult =
            GetResult(
                firstOrder,
                "Year");

        var secondResult =
            GetResult(
                secondOrder,
                "Year");

        Assert.Empty(
            firstResult.Value);

        Assert.Empty(
            secondResult.Value);

        Assert.Equal(
            MetadataConsensusStrength.Conflict,
            firstResult.Strength);

        Assert.Equal(
            MetadataConsensusStrength.Conflict,
            secondResult.Strength);

        Assert.Equal(
            0,
            firstResult.SupportingProviders);

        Assert.Equal(
            0,
            secondResult.SupportingProviders);

        Assert.Equal(
            firstResult.AgreementPercentage,
            secondResult.AgreementPercentage);

        Assert.Equal(
            3,
            firstResult.ProvidersWithValue);

        Assert.Equal(
            3,
            secondResult.ProvidersWithValue);
    }

    /// <summary>
    /// Running the same evidence repeatedly must always produce
    /// the same consensus.
    /// </summary>
    [Fact]
    public void Year_RepeatedAnalysisOfSameEvidence_ReturnsIdenticalConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    year: 2007),

                CreateEvidence(
                    "Discogs",
                    year: 2000),

                CreateEvidence(
                    "ReccoBeats",
                    year: 2000));

        var results =
            Enumerable
                .Range(
                    0,
                    10)
                .Select(
                    _ =>
                        GetResult(
                            candidates,
                            "Year"))
                .ToList();

        Assert.All(
            results,
            result =>
            {
                Assert.Equal(
                    "2000",
                    result.Value);

                Assert.Equal(
                    2,
                    result.SupportingProviders);

                Assert.Equal(
                    3,
                    result.ProvidersWithValue);

                Assert.Equal(
                    66.7,
                    result.AgreementPercentage);

                Assert.Equal(
                    MetadataConsensusStrength.Weak,
                    result.Strength);
            });
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static IReadOnlyList<MetadataEvidenceAnalysisResult>
        CreateCandidates(
            params MetadataEvidence[] evidence)
    {
        return evidence
            .Select(
                item =>
                    new MetadataEvidenceAnalysisResult
                    {
                        Evidence =
                            item,

                        Match =
                            new MetadataCandidateMatch
                            {
                                IsMatch = true,

                                Score = 100
                            }
                    })
            .ToList();
    }

    private static MetadataEvidence CreateEvidence(
        string source,
        string artist = "",
        string title = "",
        string album = "",
        string genre = "",
        int? year = null,
        double? bpm = null,
        TimeSpan? duration = null)
    {
        return new MetadataEvidence
        {
            Source =
                source,

            ExternalId =
                $"{source}-test",

            Artist =
                artist,

            Title =
                title,

            Album =
                album,

            Genre =
                genre,

            Year =
                year,

            BPM =
                bpm,

            Duration =
                duration,

            ProviderConfidence =
                100,

            MatchReason =
                "Test evidence"
        };
    }

    private static MetadataConsensusResult GetResult(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
        string field)
    {
        var results =
            _staticService.Analyse(
                candidates);

        return results.Single(
            x =>
                x.Field.Equals(
                    field,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static readonly MetadataConsensusService
        _staticService =
            new();
}