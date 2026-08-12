using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace DJLibraryManager.UI.Analysis.Modules;

/// <summary>
/// Analyses the integrity of media files.
///
/// VirtualDJ sampler files are excluded from File Integrity
/// analysis because they are managed by VirtualDJ rather than
/// being part of the user's main music library.
/// </summary>
public sealed class FileIntegrityAnalysisModule : IAnalysisModule
{
    private readonly List<AnalysisIssue> _issues = new();

    private int _trackCount;

    public string Name => "File Integrity";

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

        // --------------------------------------------------------
        // Missing path
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(media.FilePath))
        {
            _issues.Add(
                CreateIssue(
                    "MissingPath",
                    "Missing File Path",
                    media));

            return;
        }

        // --------------------------------------------------------
        // Missing file
        // --------------------------------------------------------

        if (!File.Exists(media.FilePath))
        {
            _issues.Add(
                CreateIssue(
                    "MissingFile",
                    "File Not Found",
                    media));

            return;
        }

        // --------------------------------------------------------
        // Zero-byte file
        // --------------------------------------------------------

        var file =
            new FileInfo(media.FilePath);

        if (file.Length == 0)
        {
            _issues.Add(
                CreateIssue(
                    "ZeroByte",
                    "Zero Byte File",
                    media));
        }
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
            // integrity analysis to handle it.
            return false;
        }
    }

    // ============================================================
    // Issue Creation
    // ============================================================

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

            Description =
                $"{title}: {media.Artist} - {media.Title}",

            Artist =
                media.Artist ?? string.Empty,

            TrackTitle =
                media.Title ?? string.Empty,

            FilePath =
                media.FilePath,

            CanAutoFix = false
        };
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

        var score =
            100 -
            ((double)issueCount /
             trackCount *
             100);

        return Math.Round(
            Math.Clamp(
                score,
                0,
                100),
            1);
    }
}