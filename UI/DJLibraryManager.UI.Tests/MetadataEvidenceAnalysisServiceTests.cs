using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataEvidenceAnalysisServiceTests
{
    private readonly MetadataEvidenceAnalysisService _service =
        new();

    // ============================================================
    // Multiple Independent Providers
    // ============================================================

    [Fact]
    public void Analyse_EvaluatesEachProviderIndependently()
    {
        var media =
            CreateMedia();

        var evidence =
            new List<MetadataEvidence>
            {
                CreateEvidence(
                    source: "MusicBrainz",
                    externalId: "musicbrainz-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_493),
                    bpm: 136.067),

                CreateEvidence(
                    source: "Discogs",
                    externalId: "discogs-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_000),
                    bpm: 136.0),

                CreateEvidence(
                    source: "ReccoBeats",
                    externalId: "reccobeats-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            446_466),
                    bpm: 136.0)
            };

        var results =
            _service.Analyse(
                media,
                evidence);

        Assert.Equal(
            3,
            results.Count);

        //
        // Every provider has been evaluated independently.
        //

        Assert.Contains(
            results,
            x => x.Evidence.Source == "MusicBrainz");

        Assert.Contains(
            results,
            x => x.Evidence.Source == "Discogs");

        Assert.Contains(
            results,
            x => x.Evidence.Source == "ReccoBeats");
    }

    // ============================================================
    // Viable Candidates
    // ============================================================

    [Fact]
    public void GetViableCandidates_ExcludesWeakCandidate()
    {
        var media =
            CreateMedia();

        var evidence =
            new List<MetadataEvidence>
            {
                CreateEvidence(
                    source: "MusicBrainz",
                    externalId: "musicbrainz-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_493),
                    bpm: 136.067),

                CreateEvidence(
                    source: "Discogs",
                    externalId: "discogs-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_000),
                    bpm: 136.0),

                CreateEvidence(
                    source: "ReccoBeats",
                    externalId: "reccobeats-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            446_466),
                    bpm: 136.0)
            };

        var viable =
            _service.GetViableCandidates(
                media,
                evidence);

        Assert.Equal(
            2,
            viable.Count);

        Assert.DoesNotContain(
            viable,
            x =>
                x.Evidence.ExternalId ==
                "reccobeats-1");
    }

    // ============================================================
    // Ordering
    // ============================================================

    [Fact]
    public void GetViableCandidates_OrdersByMatchScore()
    {
        var media =
            CreateMedia();

        var evidence =
            new List<MetadataEvidence>
            {
                CreateEvidence(
                    source: "MusicBrainz",
                    externalId: "musicbrainz-1",
                    duration:
                        TimeSpan.FromMilliseconds(
                            225_493),
                    bpm: 136.067),

                CreateEvidence(
                    source: "Discogs",
                    externalId: "discogs-1",
                    duration:
                        TimeSpan.FromSeconds(
                            224),
                    bpm: 135.0)
            };

        var viable =
            _service.GetViableCandidates(
                media,
                evidence);

        Assert.Equal(
            2,
            viable.Count);

        Assert.True(
            viable[0].Match.Score >=
            viable[1].Match.Score);
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static DJLMMediaItem CreateMedia()
    {
        return new DJLMMediaItem
        {
            Provider =
                "Test",

            FilePath =
                @"C:\Test\Darude - Sandstorm.mp3",

            Artist =
                "Darude",

            Title =
                "Sandstorm",

            Duration =
                TimeSpan.FromMilliseconds(
                    225_000),

            BPM =
                136,

            MediaType =
                "Audio"
        };
    }

    private static MetadataEvidence CreateEvidence(
        string source,
        string externalId,
        TimeSpan duration,
        double bpm)
    {
        return new MetadataEvidence
        {
            Source =
                source,

            ExternalId =
                externalId,

            Artist =
                "Darude",

            Title =
                "Sandstorm",

            Duration =
                duration,

            BPM =
                bpm,

            Key =
                "Bm",

            ProviderConfidence =
                100,

            MatchReason =
                "Test evidence"
        };
    }
}