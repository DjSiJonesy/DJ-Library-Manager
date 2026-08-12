using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses metadata completeness.
///
/// One AnalysisIssue is created per affected track, even when
/// multiple metadata fields are missing.
/// </summary>
public sealed class MetadataAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Metadata";

    // ============================================================
    // Begin
    // ============================================================

    public void Begin()
    {
        _trackCount = 0;

        _issues.Clear();
    }

    // ============================================================
    // Analyse
    // ============================================================

    public void Analyse(
        DJLMMediaItem media)
    {
        _trackCount++;

        var missingFields =
            new List<string>();

        // --------------------------------------------------------
        // Artist
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Artist))
        {
            missingFields.Add("Artist");
        }

        // --------------------------------------------------------
        // Title
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Title))
        {
            missingFields.Add("Title");
        }

        // --------------------------------------------------------
        // Album
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Album))
        {
            missingFields.Add("Album");
        }

        // --------------------------------------------------------
        // Genre
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Genre))
        {
            missingFields.Add("Genre");
        }

        // --------------------------------------------------------
        // Year
        // --------------------------------------------------------

        if (!media.Year.HasValue ||
            media.Year.Value <= 0)
        {
            missingFields.Add("Year");
        }

        // --------------------------------------------------------
        // BPM
        // --------------------------------------------------------

        if (!media.BPM.HasValue ||
            media.BPM.Value <= 0)
        {
            missingFields.Add("BPM");
        }

        // --------------------------------------------------------
        // Musical Key
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Key))
        {
            missingFields.Add("Key");
        }

        // --------------------------------------------------------
        // Duration
        // --------------------------------------------------------

        if (!media.Duration.HasValue ||
            media.Duration.Value <= TimeSpan.Zero)
        {
            missingFields.Add("Duration");
        }

        // --------------------------------------------------------
        // Complete track
        // --------------------------------------------------------

        if (missingFields.Count == 0)
            return;

        // --------------------------------------------------------
        // Create ONE issue for the track
        // --------------------------------------------------------

        _issues.Add(
            CreateIssue(
                media,
                missingFields));
    }

    // ============================================================
    // Complete
    // ============================================================

    public AnalysisCategoryResult Complete()
    {
        return new AnalysisCategoryResult
        {
            Name = Name,

            HealthScore =
                CalculateHealth(
                    _trackCount,
                    _issues.Count),

            Issues = _issues
        };
    }

    // ============================================================
    // Issue Creation
    // ============================================================

    private static AnalysisIssue CreateIssue(
        DJLMMediaItem media,
        IReadOnlyList<string> missingFields)
    {
        var trackName =
            BuildTrackName(media);

        var missing =
            string.Join(
                ", ",
                missingFields);

        return new AnalysisIssue
        {
            Category = "Metadata",

            Type = "MetadataIncomplete",

            Title = "Incomplete Metadata",

            Description =
                $"{trackName} — Missing: {missing}",

            FilePath =
                media.FilePath,

            MissingFields =
                missingFields,

            CanAutoFix = true
        };
    }

    // ============================================================
    // Track Name
    // ============================================================

    private static string BuildTrackName(
        DJLMMediaItem media)
    {
        var artist =
            media.Artist?.Trim()
            ?? string.Empty;

        var title =
            media.Title?.Trim()
            ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(artist) &&
            !string.IsNullOrWhiteSpace(title))
        {
            return $"{artist} - {title}";
        }

        if (!string.IsNullOrWhiteSpace(title))
            return title;

        if (!string.IsNullOrWhiteSpace(artist))
            return artist;

        return media.FilePath;
    }

    // ============================================================
    // Health
    // ============================================================

    private static double CalculateHealth(
        int trackCount,
        int issueCount)
    {
        if (trackCount == 0)
            return 100;

        var healthyTracks =
            trackCount - issueCount;

        var score =
            (double)healthyTracks /
            trackCount *
            100;

        return Math.Round(
            Math.Clamp(
                score,
                0,
                100),
            1);
    }
}