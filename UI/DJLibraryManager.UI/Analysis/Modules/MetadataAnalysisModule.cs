using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses metadata completeness.
/// </summary>
public sealed class MetadataAnalysisModule : IAnalysisModule
{
    public string Name => "Metadata";

    public Task<AnalysisCategoryResult> AnalyseAsync(
        IReadOnlyList<DJLMMediaItem> mediaItems,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<AnalysisIssue>();

        foreach (var media in mediaItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(media.Artist))
                issues.Add(CreateIssue(
                    "MissingArtist",
                    "Missing Artist",
                    media));

            if (string.IsNullOrWhiteSpace(media.Title))
                issues.Add(CreateIssue(
                    "MissingTitle",
                    "Missing Title",
                    media));

            if (string.IsNullOrWhiteSpace(media.Album))
                issues.Add(CreateIssue(
                    "MissingAlbum",
                    "Missing Album",
                    media));

            if (string.IsNullOrWhiteSpace(media.Genre))
                issues.Add(CreateIssue(
                    "MissingGenre",
                    "Missing Genre",
                    media));

            if (media.BPM is null || media.BPM <= 0)
                issues.Add(CreateIssue(
                    "MissingBPM",
                    "Missing BPM",
                    media));

            if (string.IsNullOrWhiteSpace(media.Key))
                issues.Add(CreateIssue(
                    "MissingKey",
                    "Missing Musical Key",
                    media));
        }

        double healthScore = CalculateHealth(mediaItems.Count, issues.Count);

        return Task.FromResult(
            new AnalysisCategoryResult
            {
                Name = Name,
                HealthScore = healthScore,
                Issues = issues
            });
    }

    private static AnalysisIssue CreateIssue(
        string type,
        string title,
        DJLMMediaItem media)
    {
        return new AnalysisIssue
        {
            Category = "Metadata",
            Type = type,
            Title = title,
            Description = $"{title}: {media.Artist} - {media.Title}",
            FilePath = media.FilePath,
            CanAutoFix = true
        };
    }

    private static double CalculateHealth(
        int trackCount,
        int issueCount)
    {
        if (trackCount == 0)
            return 100;

        var possibleIssues = trackCount * 6.0;

        var score = 100 - ((issueCount / possibleIssues) * 100);

        return Math.Round(Math.Clamp(score, 0, 100), 1);
    }
}