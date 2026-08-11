using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses music-related metadata quality.
/// </summary>
public sealed class MusicAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Music";

    public void Begin()
    {
        _trackCount = 0;
        _issues.Clear();
    }

    public void Analyse(DJLMMediaItem media)
    {
        _trackCount++;

        if (media.BPM is <= 0 or > 300)
        {
            _issues.Add(CreateIssue(
                "InvalidBPM",
                "Invalid BPM",
                media));
        }

        if (!string.IsNullOrWhiteSpace(media.Key))
        {
            if (!IsValidKey(media.Key))
            {
                _issues.Add(CreateIssue(
                    "InvalidKey",
                    "Invalid Musical Key",
                    media));
            }
        }

        if (media.Duration <= TimeSpan.Zero)
        {
            _issues.Add(CreateIssue(
                "InvalidDuration",
                "Invalid Duration",
                media));
        }
    }

    public AnalysisCategoryResult Complete()
    {
        return new AnalysisCategoryResult
        {
            Name = Name,
            HealthScore = CalculateHealth(_trackCount, _issues.Count),
            Issues = _issues
        };
    }

    private static bool IsValidKey(string key)
    {
        key = key.Trim().ToUpperInvariant();

        return key.EndsWith("A") ||
               key.EndsWith("B") ||
               key.Contains("MAJ") ||
               key.Contains("MIN");
    }

    private static AnalysisIssue CreateIssue(
        string type,
        string title,
        DJLMMediaItem media)
    {
        return new AnalysisIssue
        {
            Category = "Music",
            Type = type,
            Title = title,
            Description = $"{title}: {media.Artist} - {media.Title}",
            FilePath = media.FilePath,
            CanAutoFix = false
        };
    }

    private static double CalculateHealth(
        int trackCount,
        int issueCount)
    {
        if (trackCount == 0)
            return 100;

        var score = 100 - ((double)issueCount / trackCount * 100);

        return Math.Round(Math.Clamp(score, 0, 100), 1);
    }
}