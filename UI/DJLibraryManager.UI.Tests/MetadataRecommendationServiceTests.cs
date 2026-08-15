using DJLibraryManager.UI.Search.Models;
using DJLibraryManager.UI.Search.Services;

using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DJLibraryManager.UI.Tests;

public sealed class MetadataRecommendationServiceTests
{
    private readonly MetadataRecommendationService _service =
        new();

    // ============================================================
    // Strong Consensus
    // ============================================================

    [Fact]
    public void StrongConsensus_IsRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Artist",
                value: "Darude",
                supportingProviders: 3,
                providersWithValue: 3,
                agreementPercentage: 100,
                strength:
                    MetadataConsensusStrength.Strong);

        var result =
            GetResult(
                consensus);

        Assert.True(
            result.IsRecommended);

        Assert.Equal(
            "Darude",
            result.RecommendedValue);

        Assert.Equal(
            string.Empty,
            result.CurrentValue);

        Assert.Equal(
            100,
            result.AgreementPercentage);

        Assert.Equal(
            3,
            result.SupportingProviders);

        Assert.Equal(
            3,
            result.ProvidersWithValue);

        Assert.Equal(
            MetadataConsensusStrength.Strong,
            result.Strength);

        Assert.False(
            result.IsSelected);
    }

    // ============================================================
    // Moderate Consensus
    // ============================================================

    [Fact]
    public void ModerateConsensus_AboveThreshold_IsRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Year",
                value: "1999",
                supportingProviders: 3,
                providersWithValue: 4,
                agreementPercentage: 75,
                strength:
                    MetadataConsensusStrength.Moderate);

        var result =
            GetResult(
                consensus);

        Assert.True(
            result.IsRecommended);

        Assert.Equal(
            "1999",
            result.RecommendedValue);

        Assert.Equal(
            75,
            result.AgreementPercentage);
    }

    // ============================================================
    // Weak Consensus
    // ============================================================

    [Fact]
    public void WeakConsensus_IsNotRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance",
                supportingProviders: 2,
                providersWithValue: 3,
                agreementPercentage:
                    66.7,
                strength:
                    MetadataConsensusStrength.Weak);

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsRecommended);

        Assert.Empty(
            result.RecommendedValue);

        Assert.Contains(
            "threshold",
            result.Reason);
    }

    // ============================================================
    // Conflict
    // ============================================================

    [Fact]
    public void Conflict_IsNotRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance",
                supportingProviders: 1,
                providersWithValue: 3,
                agreementPercentage:
                    33.3,
                strength:
                    MetadataConsensusStrength.Conflict);

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsRecommended);

        Assert.Empty(
            result.RecommendedValue);

        Assert.Contains(
            "disagree",
            result.Reason,
            System.StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // No Data
    // ============================================================

    [Fact]
    public void NoData_IsNotRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Album",
                value: "",
                supportingProviders: 0,
                providersWithValue: 0,
                agreementPercentage: 0,
                strength:
                    MetadataConsensusStrength.NoData);

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsRecommended);

        Assert.Empty(
            result.RecommendedValue);

        Assert.Contains(
            "No provider",
            result.Reason);
    }

    // ============================================================
    // Below Threshold
    // ============================================================

    [Fact]
    public void StrongConsensus_BelowThreshold_IsNotRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Artist",
                value: "Darude",
                supportingProviders: 1,
                providersWithValue: 2,
                agreementPercentage: 50,
                strength:
                    MetadataConsensusStrength.Strong);

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsRecommended);

        Assert.Empty(
            result.RecommendedValue);

        Assert.Contains(
            "50",
            result.Reason);

        Assert.Contains(
            "75",
            result.Reason);
    }

    // ============================================================
    // Supported Fields
    // ============================================================

    [Fact]
    public void SupportedMetadataFields_AreProcessed()
    {
        var consensus =
            new[]
            {
                CreateConsensus(
                    "Artist",
                    "Darude"),

                CreateConsensus(
                    "Title",
                    "Sandstorm"),

                CreateConsensus(
                    "Album",
                    "Before the Storm"),

                CreateConsensus(
                    "Genre",
                    "Trance"),

                CreateConsensus(
                    "Year",
                    "1999"),

                CreateConsensus(
                    "BPM",
                    "136"),

                CreateConsensus(
                    "Duration",
                    "03:45")
            };

        var results =
            _service
                .Recommend(
                    consensus);

        Assert.Equal(
            7,
            results.Count);

        Assert.All(
            results,
            result =>
            {
                Assert.True(
                    result.IsRecommended);

                Assert.False(
                    string.IsNullOrWhiteSpace(
                        result.RecommendedValue));

                Assert.False(
                    result.IsSelected);
            });
    }

    // ============================================================
    // Key
    // ============================================================

    [Fact]
    public void Key_IsNotRecommendedField()
    {
        var consensus =
            CreateConsensus(
                field: "Key",
                value: "E",
                supportingProviders: 3,
                providersWithValue: 3,
                agreementPercentage: 100,
                strength:
                    MetadataConsensusStrength.Strong);

        var results =
            _service
                .Recommend(
                    new[]
                    {
                        consensus
                    });

        Assert.Empty(
            results);
    }

    // ============================================================
    // Unknown Field
    // ============================================================

    [Fact]
    public void UnknownField_IsIgnored()
    {
        var consensus =
            CreateConsensus(
                field: "Popularity",
                value: "69",
                supportingProviders: 3,
                providersWithValue: 3,
                agreementPercentage: 100,
                strength:
                    MetadataConsensusStrength.Strong);

        var results =
            _service
                .Recommend(
                    new[]
                    {
                        consensus
                    });

        Assert.Empty(
            results);
    }

    // ============================================================
    // Empty Field
    // ============================================================

    [Fact]
    public void EmptyField_IsIgnored()
    {
        var consensus =
            CreateConsensus(
                field: "",
                value: "Something",
                supportingProviders: 3,
                providersWithValue: 3,
                agreementPercentage: 100,
                strength:
                    MetadataConsensusStrength.Strong);

        var results =
            _service
                .Recommend(
                    new[]
                    {
                        consensus
                    });

        Assert.Empty(
            results);
    }

    // ============================================================
    // Empty Recommended Value
    // ============================================================

    [Fact]
    public void EmptyConsensusValue_IsNotRecommended()
    {
        var consensus =
            CreateConsensus(
                field: "Artist",
                value: "",
                supportingProviders: 3,
                providersWithValue: 3,
                agreementPercentage: 100,
                strength:
                    MetadataConsensusStrength.Strong);

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsRecommended);

        Assert.Empty(
            result.RecommendedValue);
    }

    // ============================================================
    // Selection State
    // ============================================================

    [Fact]
    public void Recommendation_IsNotSelectedByDefault()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance");

        var result =
            GetResult(
                consensus);

        Assert.True(
            result.IsRecommended);

        Assert.False(
            result.IsSelected);
    }

    [Fact]
    public void Recommendation_CanBeSelectedByUser()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance");

        var result =
            GetResult(
                consensus);

        Assert.False(
            result.IsSelected);

        result.IsSelected = true;

        Assert.True(
            result.IsSelected);
    }

    // ============================================================
    // IsChange
    // ============================================================

    [Fact]
    public void Recommendation_WithDifferentValue_IsChange()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance");

        var result =
            GetResult(
                consensus);

        var recommendation =
            new MetadataChangeRecommendation
            {
                Field =
                    result.Field,

                CurrentValue =
                    "Rock",

                RecommendedValue =
                    result.RecommendedValue,

                AgreementPercentage =
                    result.AgreementPercentage,

                SupportingProviders =
                    result.SupportingProviders,

                ProvidersWithValue =
                    result.ProvidersWithValue,

                Strength =
                    result.Strength,

                IsRecommended =
                    result.IsRecommended,

                Reason =
                    result.Reason
            };

        Assert.True(
            recommendation.IsChange);
    }

    [Fact]
    public void Recommendation_WithSameValue_IsNotChange()
    {
        var consensus =
            CreateConsensus(
                field: "Genre",
                value: "Trance");

        var result =
            GetResult(
                consensus);

        var recommendation =
            new MetadataChangeRecommendation
            {
                Field =
                    result.Field,

                CurrentValue =
                    "Trance",

                RecommendedValue =
                    result.RecommendedValue,

                AgreementPercentage =
                    result.AgreementPercentage,

                SupportingProviders =
                    result.SupportingProviders,

                ProvidersWithValue =
                    result.ProvidersWithValue,

                Strength =
                    result.Strength,

                IsRecommended =
                    result.IsRecommended,

                Reason =
                    result.Reason
            };

        Assert.False(
            recommendation.IsChange);
    }

    // ============================================================
    // Null Input
    // ============================================================

    [Fact]
    public void NullConsensus_ThrowsArgumentNullException()
    {
        Assert.Throws<System.ArgumentNullException>(
            () =>
                _service.Recommend(
                    null!));
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static MetadataConsensusResult CreateConsensus(
        string field,
        string value,
        int supportingProviders = 3,
        int providersWithValue = 3,
        double agreementPercentage = 100,
        MetadataConsensusStrength strength =
            MetadataConsensusStrength.Strong)
    {
        return new MetadataConsensusResult
        {
            Field =
                field,

            Value =
                value,

            SupportingProviders =
                supportingProviders,

            ProvidersWithValue =
                providersWithValue,

            AgreementPercentage =
                agreementPercentage,

            Strength =
                strength,

            SupportingSources =
                new List<string>
                {
                    "MusicBrainz",
                    "Discogs",
                    "ReccoBeats"
                }
                .Take(
                    supportingProviders)
                .ToList(),

            ConflictingSources =
                new List<string>()
        };
    }

    private MetadataChangeRecommendation GetResult(
        MetadataConsensusResult consensus)
    {
        return _service
            .Recommend(
                new[]
                {
                    consensus
                })
            .Single();
    }
}