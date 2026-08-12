using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses metadata completeness.
///
/// One AnalysisIssue is created per affected track, even when
/// multiple metadata fields are missing.
///
/// VirtualDJ sampler files are excluded because they are managed
/// by VirtualDJ rather than being part of the user's main
/// music library.
/// </summary>
public sealed class MetadataAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Metadata";

    // ============================================================
    // VirtualDJ Sampler Location
    // ============================================================

    private static string VirtualDJSamplerAudioPath =>
        Path.GetFullPath(
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "VirtualDJ",
                "Sampler",
                "Audio"));

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
        // --------------------------------------------------------
        // VirtualDJ sampler files are not part of the main
        // music library and should not be analysed.
        // --------------------------------------------------------

        if (IsVirtualDJSamplerFile(media.FilePath))
            return;

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

            Artist =
                media.Artist ?? string.Empty,

            TrackTitle =
                media.Title ?? string.Empty,

            FilePath =
                media.FilePath,

            MissingFields =
                missingFields,

            CanAutoFix = true
        };
    }

    // ============================================================
    // VirtualDJ Sampler Detection
    // ============================================================

    private static bool IsVirtualDJSamplerFile(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var fullPath =
                Path.GetFullPath(filePath);

            var samplerPath =
                VirtualDJSamplerAudioPath;

            if (!samplerPath.EndsWith(
                    Path.DirectorySeparatorChar))
            {
                samplerPath +=
                    Path.DirectorySeparatorChar;
            }

            return fullPath.StartsWith(
                samplerPath,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // If the path cannot be resolved, allow the normal
            // metadata analysis to handle it.
            return false;
        }
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