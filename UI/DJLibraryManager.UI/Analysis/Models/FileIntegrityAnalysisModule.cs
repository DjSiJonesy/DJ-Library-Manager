using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses the integrity of media files.
/// </summary>
public sealed class FileIntegrityAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "File Integrity";

    public void Begin()
    {
        _trackCount = 0;
        _issues.Clear();
    }

    public void Analyse(DJLMMediaItem media)
    {
        _trackCount++;

        if (string.IsNullOrWhiteSpace(media.FilePath))
        {
            _issues.Add(CreateIssue(
                "MissingPath",
                "Missing File Path",
                media));

            return;
        }

        if (!File.Exists(media.FilePath))
        {
            _issues.Add(CreateIssue(
                "MissingFile",
                "File Not Found",
                media));

            return;
        }

        var file = new FileInfo(media.FilePath);

        if (file.Length == 0)
        {
            _issues.Add(CreateIssue(
                "ZeroByte",
                "Zero Byte File",
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
            Category = "File Integrity",
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