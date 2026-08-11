using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses provider-specific information.
/// </summary>
public sealed class ProviderAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Providers";

    public void Begin()
    {
        _trackCount = 0;
        _issues.Clear();
    }

    public void Analyse(DJLMMediaItem media)
    {
        _trackCount++;

        if (string.IsNullOrWhiteSpace(media.Provider))
        {
            _issues.Add(CreateIssue(
                "MissingProvider",
                "Missing Provider",
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

    private static AnalysisIssue CreateIssue(
        string type,
        string title,
        DJLMMediaItem media)
    {
        return new AnalysisIssue
        {
            Category = "Providers",
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