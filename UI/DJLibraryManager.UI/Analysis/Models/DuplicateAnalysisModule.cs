using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Detects duplicate tracks within the library.
///
/// Analysis identifies duplicate groups. Search subsequently
/// evaluates the files within each group and recommends the
/// strongest candidate.
///
/// A track must have both Artist and Title information before
/// it can participate in duplicate detection. This prevents
/// tracks with missing metadata from being incorrectly grouped
/// together simply because they have the same duration.
/// </summary>
public sealed class DuplicateAnalysisModule : IAnalysisModule
{
    private readonly Dictionary<
        string,
        List<DJLMMediaItem>> _index = new();

    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "Duplicates";

    // ============================================================
    // Begin
    // ============================================================

    public void Begin()
    {
        _trackCount = 0;

        _index.Clear();

        _issues.Clear();
    }

    // ============================================================
    // Analyse
    // ============================================================

    public void Analyse(
        DJLMMediaItem media)
    {
        _trackCount++;

        // --------------------------------------------------------
        // Artist and Title are required for duplicate detection.
        //
        // If either is missing, Metadata Analysis is responsible
        // for reporting the problem.
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.Artist) ||
            string.IsNullOrWhiteSpace(media.Title))
        {
            return;
        }

        var fingerprint =
            CreateFingerprint(media);

        if (!_index.TryGetValue(
                fingerprint,
                out var tracks))
        {
            tracks = [];

            _index.Add(
                fingerprint,
                tracks);
        }

        tracks.Add(media);
    }

    // ============================================================
    // Complete
    // ============================================================

    public AnalysisCategoryResult Complete()
    {
        var duplicateGroups =
            _index.Values
                .Where(x => x.Count > 1)
                .ToList();

        foreach (var duplicateGroup in duplicateGroups)
        {
            CreateDuplicateIssue(
                duplicateGroup);
        }

        var duplicateTrackCount =
            duplicateGroups.Sum(
                group => group.Count);

        return new AnalysisCategoryResult
        {
            Name = Name,

            HealthScore =
                CalculateHealth(
                    _trackCount,
                    duplicateTrackCount),

            Issues = _issues
        };
    }

    // ============================================================
    // Duplicate Issue
    // ============================================================

    private void CreateDuplicateIssue(
        List<DJLMMediaItem> duplicateGroup)
    {
        if (duplicateGroup.Count < 2)
            return;

        var primary =
            duplicateGroup[0];

        var relatedPaths =
            duplicateGroup
                .Skip(1)
                .Select(x => x.FilePath)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        _issues.Add(
            new AnalysisIssue
            {
                Category = "Duplicates",

                Type = "DuplicateTrack",

                Title = "Duplicate Track",

                Description =
                    $"{primary.Artist} - {primary.Title} " +
                    $"({duplicateGroup.Count:N0} copies found)",

                FilePath =
                    primary.FilePath,

                RelatedFilePaths =
                    relatedPaths,

                CanAutoFix = false
            });
    }

    // ============================================================
    // Fingerprint
    // ============================================================

    private static string CreateFingerprint(
        DJLMMediaItem media)
    {
        return string.Join(
            "|",
            Normalise(media.Artist),
            Normalise(media.Title));
    }

    // ============================================================
    // Normalisation
    // ============================================================

    private static string Normalise(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Trim()
            .ToUpperInvariant();
    }

    // ============================================================
    // Health
    // ============================================================

    private static double CalculateHealth(
        int trackCount,
        int duplicateTrackCount)
    {
        if (trackCount == 0)
            return 100;

        var healthyTracks =
            trackCount - duplicateTrackCount;

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