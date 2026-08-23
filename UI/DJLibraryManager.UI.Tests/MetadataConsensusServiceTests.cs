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
    }

    [Fact]
    public void Artist_TwoOfThreeAgree_Returns66Point7Percent()
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
                    artist: "Other Artist"));

        var result =
            GetResult(
                candidates,
                "Artist");

        Assert.Equal(
            "Darude",
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
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);
    }

    // ============================================================
    // Album
    // ============================================================

    [Fact]
    public void Album_TwoOfThreeAgree_Returns66Point7Percent()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    album: "Before The Storm"),

                CreateEvidence(
                    "Discogs",
                    album: "Before The Storm"),

                CreateEvidence(
                    "ReccoBeats",
                    album: "Different Album"));

        var result =
            GetResult(
                candidates,
                "Album");

        Assert.Equal(
            "Before The Storm",
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
    }

    // ============================================================
    // Genre
    // ============================================================

    /// <summary>
    /// Genre deliberately permits a two-provider agreement.
    ///
    /// The consensus service reports the actual 66.7% consensus.
    /// MetadataRecommendationService will later interpret this
    /// as sufficient for Genre because two independent providers
    /// agree.
    /// </summary>
    [Fact]
    public void Genre_TwoOfThreeAgree_Returns66Point7Percent()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    genre: "Trance"),

                CreateEvidence(
                    "Discogs",
                    genre: "Trance"),

                CreateEvidence(
                    "ReccoBeats",
                    genre: "Electronic"));

        var result =
            GetResult(
                candidates,
                "Genre");

        Assert.Equal(
            "Trance",
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

        Assert.Contains(
            "ReccoBeats",
            result.ConflictingSources);
    }

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

        Assert.Empty(
            result.Value);

        Assert.Equal(
            0,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            0,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Conflict,
            result.Strength);

        Assert.Equal(
            3,
            result.ConflictingSources.Count);
    }

    // ============================================================
    // Year
    // ============================================================

    [Fact]
    public void Year_TwoOfThreeAgree_Returns66Point7Percent()
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

        Assert.Contains(
            "ReccoBeats",
            result.ConflictingSources);
    }

    // ============================================================
    // BPM
    // ============================================================

    [Fact]
    public void BPM_ExactMatch_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "Discogs",
                    bpm: 136.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 136.0));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.Equal(
            "136",
            result.Value);
    }

    [Fact]
    public void BPM_OneBpmDifference_IsAgreement()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "Discogs",
                    bpm: 136.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 137.0));

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

    [Fact]
    public void BPM_HalfBpmDifference_IsAgreement()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "Discogs",
                    bpm: 136.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 136.5));

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

    [Fact]
    public void BPM_TwoBpmDifference_IsConflict()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "Discogs",
                    bpm: 136.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 138.0));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Empty(
            result.Value);

        Assert.Equal(
            0,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            0,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Conflict,
            result.Strength);
    }

    /// <summary>
    /// MusicBrainz is deliberately not a BPM provider.
    ///
    /// Its absence must not reduce BPM agreement.
    /// </summary>
    [Fact]
    public void BPM_MusicBrainzDoesNotSupplyValue_IsIgnored()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz"),

                CreateEvidence(
                    "Discogs",
                    bpm: 136.0),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 136.5));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);
    }

    /// <summary>
    /// If Discogs has no BPM but ReccoBeats does, ReccoBeats'
    /// BPM is still usable evidence.
    /// </summary>
    [Fact]
    public void BPM_DiscogsBlank_ReccoBeatsValueIsUsable()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz"),

                CreateEvidence(
                    "Discogs"),

                CreateEvidence(
                    "ReccoBeats",
                    bpm: 136.5));

        var result =
            GetResult(
                candidates,
                "BPM");

        Assert.Equal(
            "136.5",
            result.Value);

        Assert.Equal(
            1,
            result.SupportingProviders);

        Assert.Equal(
            1,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.Contains(
            "ReccoBeats",
            result.SupportingSources);
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
                    "Discogs",
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
    // Key
    // ============================================================

    [Fact]
    public void Key_ThreeProvidersAgree_ReturnsStrongConsensus()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    key: "F#m"),

                CreateEvidence(
                    "Discogs",
                    key: "F#m"),

                CreateEvidence(
                    "ReccoBeats",
                    key: "F#m"));

        var result =
            GetResult(
                candidates,
                "Key");

        Assert.Equal(
            "F#m",
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
    // Missing Provider Data
    // ============================================================

    [Fact]
    public void Genre_MissingProviderValuesAreNotDisagreement()
    {
        var candidates =
            CreateCandidates(
                CreateEvidence(
                    "MusicBrainz",
                    genre: "Trance"),

                CreateEvidence(
                    "Discogs",
                    genre: "Trance"),

                CreateEvidence(
                    "ReccoBeats"));

        var result =
            GetResult(
                candidates,
                "Genre");

        Assert.Equal(
            "Trance",
            result.Value);

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.DoesNotContain(
            "ReccoBeats",
            result.ConflictingSources);
    }

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
    // Provider Independence
    // ============================================================

    [Fact]
    public void Genre_MultipleCandidatesFromSameProvider_CountsProviderOnce()
    {
        var candidates =
            CreateCandidates(
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

        Assert.Equal(
            "Trance",
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
            "Trance",
            result.Value);

        Assert.Equal(
            2,
            result.SupportingProviders);

        Assert.Equal(
            2,
            result.ProvidersWithValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.DoesNotContain(
            "ReccoBeats",
            result.SupportingSources);
    }

    // ============================================================
    // Determinism
    // ============================================================

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
            3,
            firstResult.ProvidersWithValue);

        Assert.Equal(
            3,
            secondResult.ProvidersWithValue);
    }

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

    private IReadOnlyList<MetadataEvidenceAnalysisResult>
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
        string key = "",
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

            Key =
                key,

            Duration =
                duration,

            ProviderConfidence =
                100,

            MatchReason =
                "Test evidence"
        };
    }

    private MetadataConsensusResult GetResult(
        IReadOnlyList<MetadataEvidenceAnalysisResult> candidates,
        string field)
    {
        var results =
            _service.Analyse(
                candidates);

        return results.Single(
            x =>
                x.Field.Equals(
                    field,
                    StringComparison.OrdinalIgnoreCase));
    }
}