using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Services;

using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Media;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services.Import;

/// <summary>
/// Imports media discovered on local storage into the DIASISS library.
///
/// Backup folders are deliberately excluded from the import scope.
/// Any directory whose name contains "backup", case-insensitively,
/// is not entered and therefore none of its contents can be imported.
/// </summary>
public sealed class MediaImportService
{
    private readonly IProgressReporter _progressReporter;
    private readonly LibraryRepository _libraryRepository;

    public MediaImportService(
        IProgressReporter progressReporter,
        LibraryRepository libraryRepository)
    {
        _progressReporter =
            progressReporter
            ?? throw new ArgumentNullException(
                nameof(progressReporter));

        _libraryRepository =
            libraryRepository
            ?? throw new ArgumentNullException(
                nameof(libraryRepository));
    }

    // ============================================================
    // Import
    // ============================================================

    /// <summary>
    /// Imports all supported media from one or more media locations.
    /// </summary>
    public async Task<MediaImportResult> ImportAsync(
        IEnumerable<MediaLocation> mediaLocations)
    {
        ArgumentNullException.ThrowIfNull(mediaLocations);

        var result =
            new MediaImportResult();

        _progressReporter.BeginOperation(
            "Import Media Library");

        //
        // Load the library once and build a fast lookup index.
        //

        var library =
            await _libraryRepository.LoadLibraryAsync();

        var existingPaths =
            await _libraryRepository.BuildPathIndexAsync();

        try
        {
            foreach (var location in mediaLocations)
            {
                if (!location.Exists)
                    continue;

                if (!Directory.Exists(location.Path))
                    continue;

                _progressReporter.ReportStage(
                    $"Scanning {location.Name}...");

                //
                // Build the list once so we know the total.
                //
                // EnumerateFilesSafe() deliberately excludes
                // directories containing "backup".
                //

                var files =
                    EnumerateFilesSafe(location.Path)
                        .Where(MediaFileTypes.IsSupported)
                        .ToList();

                var totalFiles =
                    files.Count;

                foreach (var file in files)
                {
                    result.Scanned++;

                    _progressReporter.ReportProgress(
                        result.Scanned,
                        totalFiles,
                        Path.GetFileName(file));

                    if (existingPaths.Contains(file))
                    {
                        result.Skipped++;
                        continue;
                    }

                    try
                    {
                        var mediaItem =
                            new DJLMMediaItem
                            {
                                Provider =
                                    "Discovery",

                                FilePath =
                                    file,

                                FileSize =
                                    new FileInfo(file).Length,

                                MediaType =
                                    MediaFileTypes.IsAudio(file)
                                        ? "Audio"
                                        : "Video"
                            };

                        library.Add(
                            mediaItem);

                        existingPaths.Add(
                            file);

                        result.Imported++;
                    }
                    catch
                    {
                        result.Failed++;
                    }
                }
            }

            //
            // Save the library once.
            //

            await _libraryRepository.SaveLibraryAsync(
                library);

            _progressReporter.ReportStage(
                "Finalising...");

            _progressReporter.Complete();

            return result;
        }
        catch (Exception ex)
        {
            _progressReporter.Fail(
                ex.Message);

            throw;
        }
    }

    // ============================================================
    // File Enumeration
    // ============================================================

    /// <summary>
    /// Enumerates supported filesystem files beneath a media
    /// location while deliberately excluding Backup folders.
    ///
    /// A directory is excluded when its name contains "backup",
    /// case-insensitively.
    ///
    /// Examples:
    ///
    /// Backup
    /// Backups
    /// DJ Backup
    /// DJ_Backup
    /// DJ-Backup
    /// MyBackupFiles
    /// BackupOldMusic
    ///
    /// None of these directories are entered.
    /// </summary>
    private static IEnumerable<string> EnumerateFilesSafe(
        string rootPath)
    {
        if (!Directory.Exists(rootPath))
            yield break;

        foreach (var file in EnumerateFilesInDirectory(
                     rootPath))
        {
            yield return file;
        }
    }

    /// <summary>
    /// Recursively enumerates files while controlling directory
    /// traversal so Backup folders are never entered.
    /// </summary>
    private static IEnumerable<string> EnumerateFilesInDirectory(
        string directoryPath)
    {
        IEnumerable<string> files;

        try
        {
            files =
                Directory.EnumerateFiles(
                    directoryPath,
                    "*.*",
                    SearchOption.TopDirectoryOnly);
        }
        catch
        {
            yield break;
        }

        foreach (var file in files)
        {
            yield return file;
        }

        IEnumerable<string> directories;

        try
        {
            directories =
                Directory.EnumerateDirectories(
                    directoryPath,
                    "*",
                    SearchOption.TopDirectoryOnly);
        }
        catch
        {
            yield break;
        }

        foreach (var directory in directories)
        {
            DirectoryInfo directoryInfo;

            try
            {
                directoryInfo =
                    new DirectoryInfo(directory);
            }
            catch
            {
                continue;
            }

            // ----------------------------------------------------
            // Backup exclusion
            // ----------------------------------------------------

            if (directoryInfo.Name.Contains(
                    "backup",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // ----------------------------------------------------
            // Reparse points / junctions
            // ----------------------------------------------------

            if ((directoryInfo.Attributes &
                 FileAttributes.ReparsePoint) != 0)
            {
                continue;
            }

            foreach (var file in
                     EnumerateFilesInDirectory(directory))
            {
                yield return file;
            }
        }
    }
}