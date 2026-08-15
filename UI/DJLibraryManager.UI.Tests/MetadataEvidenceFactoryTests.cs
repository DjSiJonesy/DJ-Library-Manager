using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataEvidenceFactoryTests
{
    [Fact]
    public void Create_ConvertsProviderResultToEvidence()
    {
        var result =
            new MetadataSearchProviderResult
            {
                Source =
                    "ReccoBeats",

                ExternalId =
                    "test-track-id",

                Artist =
                    "Darude",

                Title =
                    "Sandstorm",

                Album =
                    "Before the Storm",

                Genre =
                    "Trance",

                Year =
                    1999,

                ReleaseYear =
                    1999,

                BPM =
                    136.067,

                Key =
                    "Bm",

                Duration =
                    TimeSpan.FromMilliseconds(
                        225_493),

                Confidence =
                    96.5,

                MatchReason =
                    "Artist and title matched"
            };

        var evidence =
            MetadataEvidenceFactory.Create(
                result);

        Assert.Equal(
            result.Source,
            evidence.Source);

        Assert.Equal(
            result.ExternalId,
            evidence.ExternalId);

        Assert.Equal(
            result.Artist,
            evidence.Artist);

        Assert.Equal(
            result.Title,
            evidence.Title);

        Assert.Equal(
            result.Album,
            evidence.Album);

        Assert.Equal(
            result.Genre,
            evidence.Genre);

        Assert.Equal(
            result.Year,
            evidence.Year);

        Assert.Equal(
            result.ReleaseYear,
            evidence.ReleaseYear);

        Assert.Equal(
            result.BPM,
            evidence.BPM);

        Assert.Equal(
            result.Key,
            evidence.Key);

        Assert.Equal(
            result.Duration,
            evidence.Duration);

        Assert.Equal(
            result.Confidence,
            evidence.ProviderConfidence);

        Assert.Equal(
            result.MatchReason,
            evidence.MatchReason);
    }
}