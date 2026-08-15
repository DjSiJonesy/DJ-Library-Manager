using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataCandidateMatcherTests
{
    private readonly MetadataCandidateMatcher _matcher = new();

    // ============================================================
    // Darude - Sandstorm
    // ============================================================

    [Fact]
    public void Sandstorm_ExactMetadata_IsStrongMatch()
    {
        var media =
            CreateMedia(
                artist: "Darude",
                title: "Sandstorm",
                duration: TimeSpan.FromSeconds(225),
                bpm: 136);

        var evidence =
            CreateEvidence(
                artist: "Darude",
                title: "Sandstorm",
                duration: TimeSpan.FromMilliseconds(225_493),
                bpm: 136.067,
                key: "Bm");

        var result =
            _matcher.Match(
                media,
                evidence);

        Assert.True(result.IsMatch);

        Assert.Equal(
            100,
            result.ArtistScore);

        Assert.Equal(
            100,
            result.TitleScore);

        Assert.True(
            result.DurationScore >= 95);

        Assert.True(
            result.BPMScore >= 95);

        Assert.False(
            result.BPMHalfDoubleMatch);

        Assert.True(
            result.Score >= 95);
    }

    // ============================================================
    // Luude - Down Under
    // ============================================================

    [Fact]
    public void DownUnder_HalfDoubleBpm_IsStrongMatch()
    {
        var media =
            CreateMedia(
                artist: "Luude",
                title: "Down Under",
                duration: TimeSpan.FromSeconds(159),
                bpm: 86);

        var evidence =
            CreateEvidence(
                artist: "Luude, Colin Hay",
                title: "Down Under (feat. Colin Hay)",
                duration: TimeSpan.FromMilliseconds(158_774),
                bpm: 171.966,
                key: "Bm");

        var result =
            _matcher.Match(
                media,
                evidence);

        Assert.True(result.IsMatch);

        Assert.True(
            result.ArtistScore >= 90);

        Assert.True(
            result.TitleScore >= 90);

        Assert.True(
            result.DurationScore >= 95);

        Assert.True(
            result.BPMScore >= 95);

        Assert.True(
            result.BPMHalfDoubleMatch);

        Assert.True(
            result.Score >= 90);
    }

    // ============================================================
    // Different Version
    // ============================================================

    [Fact]
    public void SameTrackName_DifferentVersion_HasWeakSupportingEvidence()
    {
        var media =
            CreateMedia(
                artist: "Example Artist",
                title: "Example Track",
                duration: TimeSpan.FromMinutes(6)
                    .Add(
                        TimeSpan.FromSeconds(41)),
                bpm: 124);

        var evidence =
            CreateEvidence(
                artist: "Example Artist",
                title: "Example Track",
                duration: TimeSpan.FromMinutes(3)
                    .Add(
                        TimeSpan.FromSeconds(45)),
                bpm: 110.841,
                key: "G");

        var result =
            _matcher.Match(
                media,
                evidence);

        Assert.Equal(
            100,
            result.ArtistScore);

        Assert.Equal(
            100,
            result.TitleScore);

        Assert.Equal(
            0,
            result.DurationScore);

        Assert.Equal(
            0,
            result.BPMScore);

        Assert.False(
            result.BPMHalfDoubleMatch);

        //
        // The candidate should not pass the 85% match threshold
        // when Artist and Title match but both Duration and BPM
        // indicate a different recording/version.
        //

        Assert.True(
            result.Score < 85,
            $"Unexpected score: {result.Score}");

        Assert.True(
            result.DurationScore == 0,
            $"Unexpected duration score: {result.DurationScore}");

        Assert.True(
            result.BPMScore == 0,
            $"Unexpected BPM score: {result.BPMScore}");

        Assert.False(
            result.IsMatch);
    }

    // ============================================================
    // Key Conflict
    // ============================================================

    [Fact]
    public void KeyDifference_DoesNotAffectCandidateMatch()
    {
        var media =
            CreateMedia(
                artist: "Darude",
                title: "Sandstorm",
                duration: TimeSpan.FromSeconds(225),
                bpm: 136);

        var evidence =
            CreateEvidence(
                artist: "Darude",
                title: "Sandstorm",
                duration: TimeSpan.FromMilliseconds(225_493),
                bpm: 136.067,
                key: "Bm");

        var result =
            _matcher.Match(
                media,
                evidence);

        //
        // The local file may say E while ReccoBeats says Bm.
        // Key is deliberately not part of candidate identity
        // matching.
        //

        Assert.True(
            result.IsMatch);

        Assert.True(
            result.Score >= 95);

        Assert.True(
            result.BPMScore >= 95);
    }

    // ============================================================
    // Missing Supporting Metadata
    // ============================================================

    [Fact]
    public void MissingDurationAndBpm_DoNotAutomaticallyRejectIdentityMatch()
    {
        var media =
            CreateMedia(
                artist: "Darude",
                title: "Sandstorm",
                duration: null,
                bpm: null);

        var evidence =
            CreateEvidence(
                artist: "Darude",
                title: "Sandstorm",
                duration: null,
                bpm: null,
                key: "Bm");

        var result =
            _matcher.Match(
                media,
                evidence);

        Assert.True(
            result.IsMatch);

        Assert.Equal(
            100,
            result.ArtistScore);

        Assert.Equal(
            100,
            result.TitleScore);
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static DJLMMediaItem CreateMedia(
        string artist,
        string title,
        TimeSpan? duration,
        double? bpm)
    {
        return new DJLMMediaItem
        {
            Provider = "Test",

            FilePath =
                @"C:\Test\Track.mp3",

            Artist =
                artist,

            Title =
                title,

            Duration =
                duration,

            BPM =
                bpm,

            MediaType =
                "Audio"
        };
    }

    private static MetadataEvidence CreateEvidence(
        string artist,
        string title,
        TimeSpan? duration,
        double? bpm,
        string key)
    {
        return new MetadataEvidence
        {
            Source =
                "TestProvider",

            ExternalId =
                "test-id",

            Artist =
                artist,

            Title =
                title,

            Duration =
                duration,

            BPM =
                bpm,

            Key =
                key,

            ProviderConfidence =
                100,

            MatchReason =
                "Test evidence"
        };
    }
}