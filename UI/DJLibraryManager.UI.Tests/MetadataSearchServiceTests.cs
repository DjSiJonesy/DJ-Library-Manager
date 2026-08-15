using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataSearchServiceTests
{
    // ============================================================
    // Independent Provider Searching
    // ============================================================

    [Fact]
    public async Task SearchAsync_QueriesEveryProviderIndependently()
    {
        var providerA =
            new FakeMetadataProvider(
                "Provider A",
                CreateResult(
                    "Provider A",
                    "Darude",
                    "Sandstorm"));

        var providerB =
            new FakeMetadataProvider(
                "Provider B",
                CreateResult(
                    "Provider B",
                    "Darude",
                    "Sandstorm"));

        var providerC =
            new FakeMetadataProvider(
                "Provider C",
                CreateResult(
                    "Provider C",
                    "Darude",
                    "Sandstorm"));

        var service =
            CreateService(
                providerA,
                providerB,
                providerC);

        var issue =
            CreateIssue();

        var results =
            await service.SearchAsync(issue);

        Assert.True(
            providerA.SearchCount > 0);

        Assert.True(
            providerB.SearchCount > 0);

        Assert.True(
            providerC.SearchCount > 0);
    }

    // ============================================================
    // Provider Results Become Search Results
    // ============================================================

    [Fact]
    public async Task SearchAsync_ReturnsProviderCandidates()
    {
        var provider =
            new FakeMetadataProvider(
                "ReccoBeats",
                CreateResult(
                    "ReccoBeats",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var service =
            CreateService(
                provider);

        var results =
            await service.SearchAsync(
                CreateIssue());

        var result =
            Assert.Single(
                results);

        Assert.Equal(
            "ReccoBeats",
            result.Source);

        Assert.Equal(
            "Darude",
            result.Artist);

        Assert.Equal(
            "Sandstorm",
            result.TrackTitle);

        Assert.Equal(
            "Trance",
            result.Genre);
    }

    // ============================================================
    // Strong Consensus
    // ============================================================

    [Fact]
    public async Task SearchAsync_StrongConsensus_CreatesMetadataRecommendation()
    {
        var providerA =
            new FakeMetadataProvider(
                "MusicBrainz",
                CreateResult(
                    "MusicBrainz",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance",
                    year: 1999,
                    bpm: 136));

        var providerB =
            new FakeMetadataProvider(
                "Discogs",
                CreateResult(
                    "Discogs",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance",
                    year: 1999,
                    bpm: 136.067));

        var providerC =
            new FakeMetadataProvider(
                "ReccoBeats",
                CreateResult(
                    "ReccoBeats",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance",
                    year: 1999,
                    bpm: 136.065));

        var issue =
            CreateIssue();

        var service =
            CreateService(
                providerA,
                providerB,
                providerC);

        var results =
            await service.SearchAsync(
                issue);

        // Provider candidates are still returned as evidence.
        Assert.Equal(
            3,
            results.Count);

        // Provider candidates are NOT themselves the metadata
        // recommendation.
        Assert.All(
            results,
            result =>
                Assert.False(
                    result.IsRecommended));

        // The metadata recommendation is now attached to the issue.
        var genreRecommendation =
            Assert.Single(
                issue.MetadataRecommendations
                    .Where(
                        x =>
                            x.Field.Equals(
                                "Genre",
                                StringComparison.OrdinalIgnoreCase)));

        Assert.True(
            genreRecommendation.IsRecommended);

        Assert.Equal(
            "Trance",
            genreRecommendation.RecommendedValue);

        Assert.Equal(
            string.Empty,
            genreRecommendation.CurrentValue);

        Assert.Equal(
            100,
            genreRecommendation.AgreementPercentage);

        Assert.Equal(
            3,
            genreRecommendation.SupportingProviders);

        Assert.Equal(
            3,
            genreRecommendation.ProvidersWithValue);

        Assert.False(
            genreRecommendation.IsSelected);
    }

    // ============================================================
    // Conflicting Metadata
    // ============================================================

    [Fact]
    public async Task SearchAsync_ConflictingMetadata_DoesNotRecommendConflictingField()
    {
        var providerA =
            new FakeMetadataProvider(
                "MusicBrainz",
                CreateResult(
                    "MusicBrainz",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var providerB =
            new FakeMetadataProvider(
                "Discogs",
                CreateResult(
                    "Discogs",
                    "Darude",
                    "Sandstorm",
                    genre: "Electronic"));

        var providerC =
            new FakeMetadataProvider(
                "ReccoBeats",
                CreateResult(
                    "ReccoBeats",
                    "Darude",
                    "Sandstorm",
                    genre: "Dance"));

        var issue =
            CreateIssue();

        var service =
            CreateService(
                providerA,
                providerB,
                providerC);

        var results =
            await service.SearchAsync(
                issue);

        // All provider candidates remain available as evidence.
        Assert.Equal(
            3,
            results.Count);

        // None of the provider candidates should be presented as
        // the overall metadata recommendation.
        Assert.All(
            results,
            result =>
                Assert.False(
                    result.IsRecommended));

        // The conflicting Genre field may still produce a
        // MetadataChangeRecommendation so the UI can explain why
        // the field could not safely be resolved.
        //
        // However, it must NEVER be marked as recommended.

        var genreRecommendation =
            Assert.Single(
                issue.MetadataRecommendations
                    .Where(
                        recommendation =>
                            recommendation.Field.Equals(
                                "Genre",
                                StringComparison.OrdinalIgnoreCase)));

        Assert.False(
            genreRecommendation.IsRecommended);

        Assert.Empty(
            genreRecommendation.RecommendedValue);

        // No conflicting provider value may be presented as the
        // recommended replacement.

        Assert.NotEqual(
            "Trance",
            genreRecommendation.RecommendedValue,
            StringComparer.OrdinalIgnoreCase);

        Assert.NotEqual(
            "Electronic",
            genreRecommendation.RecommendedValue,
            StringComparer.OrdinalIgnoreCase);

        Assert.NotEqual(
            "Dance",
            genreRecommendation.RecommendedValue,
            StringComparer.OrdinalIgnoreCase);
    }

    // ============================================================
    // Provider Failure Isolation
    // ============================================================

    [Fact]
    public async Task SearchAsync_WhenOneProviderFails_OtherProvidersStillReturnResults()
    {
        var failingProvider =
            new FakeMetadataProvider(
                "Failing Provider",
                exception:
                    new InvalidOperationException(
                        "Test provider failure"));

        var workingProvider =
            new FakeMetadataProvider(
                "Working Provider",
                CreateResult(
                    "Working Provider",
                    "Darude",
                    "Sandstorm"));

        var service =
            CreateService(
                failingProvider,
                workingProvider);

        var results =
            await service.SearchAsync(
                CreateIssue());

        Assert.Single(
            results);

        Assert.Equal(
            "Working Provider",
            results[0].Source);
    }

    // ============================================================
    // Empty Provider Results
    // ============================================================

    [Fact]
    public async Task SearchAsync_WhenProvidersReturnNothing_ReturnsNoResults()
    {
        var providerA =
            new FakeMetadataProvider(
                "Provider A");

        var providerB =
            new FakeMetadataProvider(
                "Provider B");

        var service =
            CreateService(
                providerA,
                providerB);

        var results =
            await service.SearchAsync(
                CreateIssue());

        Assert.Empty(
            results);
    }

    // ============================================================
    // Missing File Path
    // ============================================================

    [Fact]
    public async Task SearchAsync_WhenFilePathMissing_ReturnsNoResults()
    {
        var provider =
            new FakeMetadataProvider(
                "Provider",
                CreateResult(
                    "Provider",
                    "Darude",
                    "Sandstorm"));

        var service =
            CreateService(
                provider);

        var issue =
            CreateIssue();

        issue.FilePath =
            string.Empty;

        var results =
            await service.SearchAsync(
                issue);

        Assert.Empty(
            results);

        Assert.Equal(
            0,
            provider.SearchCount);
    }

    // ============================================================
    // Original Issue Metadata Is Not Modified
    // ============================================================

    [Fact]
    public async Task SearchAsync_DoesNotModifyOriginalIssueMetadata()
    {
        var provider =
            new FakeMetadataProvider(
                "Provider",
                CreateResult(
                    "Provider",
                    "Different Artist",
                    "Different Title",
                    genre: "Trance"));

        var issue =
            CreateIssue();

        var originalArtist =
            issue.Artist;

        var originalTitle =
            issue.TrackTitle;

        var originalAlbum =
            issue.Album;

        var service =
            CreateService(
                provider);

        await service.SearchAsync(
            issue);

        Assert.Equal(
            originalArtist,
            issue.Artist);

        Assert.Equal(
            originalTitle,
            issue.TrackTitle);

        Assert.Equal(
            originalAlbum,
            issue.Album);
    }

    // ============================================================
    // Recommendations Are Separate From Provider Results
    // ============================================================

    [Fact]
    public async Task SearchAsync_ProviderResultsAndMetadataRecommendationsAreSeparate()
    {
        var providerA =
            new FakeMetadataProvider(
                "MusicBrainz",
                CreateResult(
                    "MusicBrainz",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var providerB =
            new FakeMetadataProvider(
                "Discogs",
                CreateResult(
                    "Discogs",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var providerC =
            new FakeMetadataProvider(
                "ReccoBeats",
                CreateResult(
                    "ReccoBeats",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var issue =
            CreateIssue();

        var service =
            CreateService(
                providerA,
                providerB,
                providerC);

        var results =
            await service.SearchAsync(
                issue);

        Assert.Equal(
            3,
            results.Count);

        Assert.All(
            results,
            result =>
                Assert.False(
                    result.IsRecommended));

        Assert.Contains(
            issue.MetadataRecommendations,
            recommendation =>
                recommendation.Field.Equals(
                    "Genre",
                    StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Recommendations Are Cleared Before A New Search
    // ============================================================

    [Fact]
    public async Task SearchAsync_ClearsPreviousMetadataRecommendations()
    {
        var provider =
            new FakeMetadataProvider(
                "Provider",
                CreateResult(
                    "Provider",
                    "Darude",
                    "Sandstorm",
                    genre: "Trance"));

        var issue =
            CreateIssue();

        var service =
            CreateService(
                provider);

        await service.SearchAsync(
            issue);

        var firstRecommendationCount =
            issue.MetadataRecommendations.Count;

        Assert.True(
            firstRecommendationCount > 0);

        await service.SearchAsync(
            issue);

        Assert.Equal(
            firstRecommendationCount,
            issue.MetadataRecommendations.Count);
    }

    // ============================================================
    // Cancellation
    // ============================================================

    [Fact]
    public async Task SearchAsync_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        var provider =
            new FakeMetadataProvider(
                "Provider",
                CreateResult(
                    "Provider",
                    "Darude",
                    "Sandstorm"));

        var service =
            CreateService(
                provider);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                service.SearchAsync(
                    CreateIssue(),
                    cancellation.Token));
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static MetadataSearchService CreateService(
        params IMetadataSearchProvider[] providers)
    {
        return new MetadataSearchService(
            providers);
    }

    private static SearchIssue CreateIssue()
    {
        return new SearchIssue
        {
            Id =
                "test-issue",

            Category =
                "Metadata",

            Type =
                "MissingGenre",

            Title =
                "Missing Genre",

            Artist =
                "Darude",

            TrackTitle =
                "Sandstorm",

            Album =
                "Before the Storm",

            FilePath =
                @"C:\Music\Darude - Sandstorm.mp3",

            Duration =
                TimeSpan.FromMilliseconds(
                    225_493)
        };
    }

    private static MetadataSearchProviderResult CreateResult(
        string source,
        string artist,
        string title,
        string album = "Before the Storm",
        string genre = "",
        int? year = null,
        double? bpm = null)
    {
        return new MetadataSearchProviderResult
        {
            Source =
                source,

            ExternalId =
                $"{source}-test-id",

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
                string.Empty,

            Duration =
                TimeSpan.FromMilliseconds(
                    225_493),

            Confidence =
                100,

            MatchReason =
                "Test candidate"
        };
    }

    // ============================================================
    // Fake Provider
    // ============================================================

    private sealed class FakeMetadataProvider
        : IMetadataSearchProvider
    {
        private readonly IReadOnlyList<
            MetadataSearchProviderResult> _results;

        private readonly Exception? _exception;

        public FakeMetadataProvider(
            string name,
            params MetadataSearchProviderResult[] results)
        {
            Name =
                name;

            _results =
                results;
        }

        public FakeMetadataProvider(
            string name,
            Exception exception)
        {
            Name =
                name;

            _results =
                Array.Empty<
                    MetadataSearchProviderResult>();

            _exception =
                exception;
        }

        public string Name { get; }

        public int SearchCount { get; private set; }

        public Task<IReadOnlyList<
            MetadataSearchProviderResult>> SearchAsync(
            MetadataSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            SearchCount++;

            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(
                _results);
        }
    }
}