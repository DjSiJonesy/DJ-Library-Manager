using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        var filenameSearchHint =
            CreateFilenameSearchHint(
                media,
                missingFields);

        return new AnalysisIssue
        {
            Category = "Metadata",

            Type = "MetadataIncomplete",

            Title = "Incomplete Metadata",

            Description =
                BuildDescription(
                    trackName,
                    missing,
                    filenameSearchHint),

            // ----------------------------------------------------
            // Existing library metadata
            //
            // These values are the actual metadata currently
            // stored in DIASISS. They are deliberately kept
            // separate from filename-derived search hints.
            // ----------------------------------------------------

            Artist =
                media.Artist ?? string.Empty,

            TrackTitle =
                media.Title ?? string.Empty,

            Album =
                media.Album ?? string.Empty,

            Duration =
                media.Duration,

            FilePath =
                media.FilePath,

            // ----------------------------------------------------
            // Filename-derived search information
            //
            // This is search evidence only and is never treated
            // as confirmed Artist or Title metadata.
            // ----------------------------------------------------

            FilenameSearchHint =
                filenameSearchHint,

            MissingFields =
                missingFields,

            CanAutoFix = true
        };
    }

    // ============================================================
    // Filename Search Hint
    // ============================================================

    /// <summary>
    /// Creates search hints from the filename when Artist or Title
    /// metadata is missing.
    ///
    /// The filename is deliberately treated as ambiguous.
    ///
    /// For:
    ///
    ///     Artist - Title.mp3
    ///
    /// we create both:
    ///
    ///     Artist = Artist
    ///     Title  = Title
    ///
    /// and:
    ///
    ///     Artist = Title
    ///     Title  = Artist
    ///
    /// Search is responsible for determining which interpretation
    /// produces the strongest external metadata match.
    /// </summary>
    private static FilenameSearchHint?
        CreateFilenameSearchHint(
            DJLMMediaItem media,
            IReadOnlyList<string> missingFields)
    {
        var artistMissing =
            missingFields.Contains(
                "Artist",
                StringComparer.OrdinalIgnoreCase);

        var titleMissing =
            missingFields.Contains(
                "Title",
                StringComparer.OrdinalIgnoreCase);

        // --------------------------------------------------------
        // If both Artist and Title already exist, there is no need
        // for filename-derived search information.
        // --------------------------------------------------------

        if (!artistMissing &&
            !titleMissing)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
                media.FilePath))
        {
            return null;
        }

        var filename =
            Path.GetFileName(
                media.FilePath);

        if (string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }

        var cleanedFilename =
            CleanFilename(
                filename);

        if (string.IsNullOrWhiteSpace(
                cleanedFilename))
        {
            return null;
        }

        var separatorIndex =
            FindFilenameSeparator(
                cleanedFilename);

        // --------------------------------------------------------
        // We need two meaningful parts before we can safely create
        // Artist/Title interpretations.
        // --------------------------------------------------------

        if (separatorIndex <= 0 ||
            separatorIndex >=
                cleanedFilename.Length - 3)
        {
            return new FilenameSearchHint
            {
                Filename =
                    filename,

                CleanedFilename =
                    cleanedFilename,

                PartA =
                    cleanedFilename,

                PartB =
                    string.Empty,

                Candidates =
                    []
            };
        }

        var partA =
            cleanedFilename[..separatorIndex]
                .Trim();

        var partB =
            cleanedFilename[(separatorIndex + 3)..]
                .Trim();

        if (string.IsNullOrWhiteSpace(partA) ||
            string.IsNullOrWhiteSpace(partB))
        {
            return new FilenameSearchHint
            {
                Filename =
                    filename,

                CleanedFilename =
                    cleanedFilename,

                PartA =
                    partA,

                PartB =
                    partB,

                Candidates =
                    []
            };
        }

        var candidates =
            new List<FilenameSearchCandidate>();

        // --------------------------------------------------------
        // Interpretation 1:
        //
        // Artist - Title
        // --------------------------------------------------------

        candidates.Add(
            new FilenameSearchCandidate
            {
                Artist =
                    partA,

                Title =
                    partB,

                Interpretation =
                    "Filename interpreted as Artist - Title"
            });

        // --------------------------------------------------------
        // Interpretation 2:
        //
        // Title - Artist
        // --------------------------------------------------------

        candidates.Add(
            new FilenameSearchCandidate
            {
                Artist =
                    partB,

                Title =
                    partA,

                Interpretation =
                    "Filename interpreted as Title - Artist"
            });

        return new FilenameSearchHint
        {
            Filename =
                filename,

            CleanedFilename =
                cleanedFilename,

            PartA =
                partA,

            PartB =
                partB,

            Candidates =
                candidates
        };
    }

    // ============================================================
    // Filename Separator
    // ============================================================

    private static int FindFilenameSeparator(
        string filename)
    {
        // --------------------------------------------------------
        // Most common DJ convention:
        //
        // Artist - Title
        //
        // Use " - " rather than a bare hyphen so that normal
        // hyphens inside titles are not incorrectly treated as
        // separators.
        // --------------------------------------------------------

        var index =
            filename.IndexOf(
                " - ",
                StringComparison.Ordinal);

        if (index >= 0)
            return index;

        // --------------------------------------------------------
        // Support an en-dash as another common filename separator.
        // --------------------------------------------------------

        return filename.IndexOf(
            " – ",
            StringComparison.Ordinal);
    }

    // ============================================================
    // Filename Cleanup
    // ============================================================

    private static string CleanFilename(
        string filename)
    {
        var extension =
            Path.GetExtension(
                filename);

        var result =
            !string.IsNullOrWhiteSpace(extension)
                ? filename[..^extension.Length]
                : filename;

        result =
            result.Trim();

        // --------------------------------------------------------
        // Remove recognised technical suffixes repeatedly.
        //
        // Examples:
        //
        // (720p60fps)
        // (1080p)
        // [720p]
        // --------------------------------------------------------

        while (true)
        {
            var original =
                result;

            result =
                RemoveTrailingBracketedTechnicalValue(
                    result);

            if (string.Equals(
                    original,
                    result,
                    StringComparison.Ordinal))
            {
                break;
            }
        }

        return result.Trim();
    }

    private static string
        RemoveTrailingBracketedTechnicalValue(
            string value)
    {
        var trimmed =
            value.TrimEnd();

        if (trimmed.EndsWith(")"))
        {
            var openIndex =
                trimmed.LastIndexOf('(');

            if (openIndex >= 0)
            {
                var contents =
                    trimmed[(openIndex + 1)..^1]
                        .Trim();

                if (IsTechnicalValue(contents))
                {
                    return trimmed[..openIndex]
                        .TrimEnd();
                }
            }
        }

        if (trimmed.EndsWith("]"))
        {
            var openIndex =
                trimmed.LastIndexOf('[');

            if (openIndex >= 0)
            {
                var contents =
                    trimmed[(openIndex + 1)..^1]
                        .Trim();

                if (IsTechnicalValue(contents))
                {
                    return trimmed[..openIndex]
                        .TrimEnd();
                }
            }
        }

        return value;
    }

    private static bool IsTechnicalValue(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var lower =
            value.Trim()
                .ToLowerInvariant();

        return
            lower.Contains("fps") ||
            lower.Contains("720p") ||
            lower.Contains("1080p") ||
            lower.Contains("2160p") ||
            lower.Contains("4k") ||
            lower.Contains("video") ||
            lower.Contains("web-dl") ||
            lower.Contains("webrip") ||
            lower.Contains("bluray") ||
            lower.Contains("brrip") ||
            lower.Contains("dvdrip");
    }

    // ============================================================
    // Description
    // ============================================================

    private static string BuildDescription(
        string trackName,
        string missing,
        FilenameSearchHint? filenameSearchHint)
    {
        var description =
            $"{trackName} — Missing: {missing}";

        if (filenameSearchHint is null)
        {
            return description;
        }

        if (filenameSearchHint.Candidates.Count == 0)
        {
            return
                $"{description} — Filename search information: " +
                $"{filenameSearchHint.CleanedFilename}";
        }

        var candidate =
            filenameSearchHint.Candidates[0];

        return
            $"{description} — Filename search: " +
            $"{candidate.Artist} - {candidate.Title}";
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