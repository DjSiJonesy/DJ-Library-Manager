using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Detects duplicate tracks within the library.
/// </summary>
public sealed class DuplicateAnalysisModule : IAnalysisModule
{
    private readonly Dictionary<string, List<DJLMMediaItem>> _index = new();
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Duplicates";

    public void Begin()
    {
        _trackCount = 0;
        _index.Clear();
        _issues.Clear();
    }

    public void Analyse(DJLMMediaItem media)
    {
        _trackCount++;

        var fingerprint = CreateFingerprint(media);

        if (!_index.TryGetValue(fingerprint, out var tracks))
        {
            tracks = [];
            _index.Add(fingerprint, tracks);
        }

        tracks.Add(media);
    }

    public AnalysisCategoryResult Complete()
    {
        foreach (var duplicateGroup in _index.Values.Where(x => x.Count > 1))
        {
            foreach (var media in duplicateGroup.Skip(1))
            {
                _issues.Add(new AnalysisIssue
                {
                    Category = "Duplicates",
                    Type = "DuplicateTrack",
                    Title = "Duplicate Track",
                    Description = $"{media.Artist} - {media.Title}",
                    FilePath = media.FilePath,
                    CanAutoFix = false
                });
            }
        }

        return new AnalysisCategoryResult
        {
            Name = Name,
            HealthScore = CalculateHealth(_trackCount, _issues.Count),
            Issues = _issues
        };
    }

    private static string CreateFingerprint(DJLMMediaItem media)
    {
        return string.Join("|",
            media.Artist?.Trim().ToUpperInvariant() ?? string.Empty,
            media.Title?.Trim().ToUpperInvariant() ?? string.Empty,
            media.Duration?.ToString() ?? string.Empty);
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