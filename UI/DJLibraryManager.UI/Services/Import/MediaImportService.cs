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
/// </summary>
public sealed class MediaImportService
{
    private readonly IProgressReporter _progressReporter;
    private readonly LibraryRepository _libraryRepository;

    public MediaImportService(
        IProgressReporter progressReporter,
        LibraryRepository libraryRepository)
    {
        _progressReporter = progressReporter
            ?? throw new ArgumentNullException(nameof(progressReporter));

        _libraryRepository = libraryRepository
            ?? throw new ArgumentNullException(nameof(libraryRepository));
    }

    /// <summary>
    /// Imports all supported media from one or more media locations.
    /// </summary>
    public async Task<MediaImportResult> ImportAsync(
        IEnumerable<MediaLocation> mediaLocations)
    {
        ArgumentNullException.ThrowIfNull(mediaLocations);

        var result = new MediaImportResult();

        _progressReporter.BeginOperation("Import Media Library");

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

                var files = EnumerateFiles(location)
                    .Where(MediaFileTypes.IsSupported)
                    .ToList();

                var totalFiles = files.Count;

                foreach (var file in files)
                {
                    result.Scanned++;

                    _progressReporter.ReportProgress(
                        result.Scanned,
                        totalFiles,
                        Path.GetFileName(file));

                    if (await _libraryRepository.MediaExistsAsync(file))
                    {
                        result.Skipped++;
                        continue;
                    }

                    try
                    {
                        var mediaItem = new DJLMMediaItem
                        {
                            Provider = "Discovery",
                            FilePath = file,
                            FileSize = new FileInfo(file).Length,
                            MediaType = MediaFileTypes.IsAudio(file)
                                ? "Audio"
                                : "Video"
                        };

                        await _libraryRepository.AddMediaItemAsync(mediaItem);

                        result.Imported++;
                    }
                    catch
                    {
                        result.Failed++;
                    }
                }
            }

            _progressReporter.ReportStage("Finalising...");

            _progressReporter.Complete();

            return result;
        }
        catch (Exception ex)
        {
            _progressReporter.Fail(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Enumerates every file beneath a media location.
    /// </summary>
    private static IEnumerable<string> EnumerateFiles(
        MediaLocation mediaLocation)
    {
        return Directory.EnumerateFiles(
            mediaLocation.Path,
            "*.*",
            SearchOption.AllDirectories);
    }
}