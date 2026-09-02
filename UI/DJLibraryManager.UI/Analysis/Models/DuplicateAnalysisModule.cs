using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
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
///
/// Files located inside folders whose name contains "backup"
/// are deliberately excluded from duplicate analysis. This
/// provides an additional safety barrier for backup libraries
/// that may have entered the DIASISS library through a provider
/// import or an existing library record.
///
/// Files that no longer exist at their recorded physical path
/// are also excluded from duplicate detection. Missing-file
/// records remain available to File Integrity Analysis but must
/// not participate in active duplicate groups.
///
/// Files located inside the DIASISS Duplicates folder are
/// deliberately excluded from duplicate analysis. These files
/// have already been moved out of the active library location
/// during Improve and remain available for recovery.
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
        // Backup protection
        //
        // Any file located beneath a directory whose name
        // contains "backup" is excluded from duplicate analysis.
        //
        // Examples:
        //
        // DJ_Library_Backup
        // DJ Backup
        // DJ-Backup
        // MyBackup
        // Backups
        // BackupOldMusic
        //
        // The filename itself is not checked. Only directory
        // names are considered.
        // --------------------------------------------------------

        if (IsInsideBackupFolder(media.FilePath))
        {
            return;
        }

        // --------------------------------------------------------
        // DIASISS Duplicates protection
        //
        // Files that have already been moved into the DIASISS
        // Duplicates folder must not participate in duplicate
        // analysis.
        //
        // The files remain in the library/database so that they
        // can be recovered later, but they are no longer active
        // physical library files.
        // --------------------------------------------------------

        if (IsInsideDiasissDuplicatesFolder(media.FilePath))
        {
            return;
        }

        // --------------------------------------------------------
        // Physical file existence
        //
        // A missing physical file must not participate in
        // duplicate detection.
        //
        // File Integrity Analysis remains responsible for
        // identifying the missing file.
        // --------------------------------------------------------

        if (!FileExists(media.FilePath))
        {
            return;
        }

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

                MediaId =
                    primary.MediaId,

                Description =
                    $"{primary.Artist} - {primary.Title} " +
                    $"({duplicateGroup.Count:N0} copies found)",

                Artist =
                    primary.Artist ?? string.Empty,

                TrackTitle =
                    primary.Title ?? string.Empty,

                FilePath =
                    primary.FilePath,

                RelatedFilePaths =
                    relatedPaths,

                CanAutoFix = false
            });
    }

    // ============================================================
    // File Existence
    // ============================================================

    /// <summary>
    /// Determines whether the recorded physical file exists.
    ///
    /// Missing files are deliberately excluded from duplicate
    /// detection. File Integrity Analysis remains responsible
    /// for reporting missing files.
    /// </summary>
    private static bool FileExists(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // DIASISS Duplicates Detection
    // ============================================================

    /// <summary>
    /// Determines whether a file is located inside the
    /// configured DIASISS Duplicates folder.
    ///
    /// Files in this folder have already been moved out of the
    /// active library during Improve. They remain available for
    /// recovery and must therefore remain in the database, but
    /// they must not participate in future duplicate analysis.
    /// </summary>
    private static bool IsInsideDiasissDuplicatesFolder(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var duplicatesFolder =
            ApplicationPaths.DiasissDuplicates;

        if (string.IsNullOrWhiteSpace(duplicatesFolder))
            return false;

        try
        {
            var fullFilePath =
                Path.GetFullPath(filePath)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            var fullDuplicatesFolder =
                Path.GetFullPath(duplicatesFolder)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            return string.Equals(
                       fullFilePath,
                       fullDuplicatesFolder,
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   fullFilePath.StartsWith(
                       fullDuplicatesFolder +
                       Path.DirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   fullFilePath.StartsWith(
                       fullDuplicatesFolder +
                       Path.AltDirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // Backup Detection
    // ============================================================

    /// <summary>
    /// Determines whether a file is located inside a directory
    /// whose name contains "backup", case-insensitively.
    ///
    /// Only directory names are checked. The filename itself is
    /// deliberately ignored.
    /// </summary>
    private static bool IsInsideBackupFolder(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var directory =
                Path.GetDirectoryName(filePath);

            while (!string.IsNullOrWhiteSpace(directory))
            {
                var directoryName =
                    Path.GetFileName(
                        directory.TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar));

                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    directoryName.Contains(
                        "backup",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var parent =
                    Directory.GetParent(directory);

                if (parent is null)
                    break;

                directory =
                    parent.FullName;
            }
        }
        catch
        {
            // If the path cannot be inspected, do not allow
            // it to be incorrectly identified as a backup.
            // Other analysis modules can still report problems
            // with the file/path.
        }

        return false;
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