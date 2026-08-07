using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Services;

using DJLibraryManager.UI.Models.Media;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services.Import;

/// <summary>
/// Imports media discovered on local storage into the DIASISS library.
/// </summary>
public sealed class MediaImportService
{
    private readonly LibraryRepository _libraryRepository;

    public MediaImportService(
        LibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository
            ?? throw new ArgumentNullException(nameof(libraryRepository));
    }

    /// <summary>
    /// Imports all supported media from one or more media locations.
    /// </summary>
    public async Task ImportAsync(
        IEnumerable<MediaLocation> mediaLocations)
    {
        ArgumentNullException.ThrowIfNull(mediaLocations);

        foreach (var location in mediaLocations)
        {
            if (!location.Exists)
                continue;

            if (!Directory.Exists(location.Path))
                continue;

            foreach (var file in EnumerateFiles(location))
            {
                //
                // Ignore anything that DIASISS doesn't support.
                //

                if (!MediaFileTypes.IsSupported(file))
                    continue;

                //
                // Skip files already imported.
                //

                if (await _libraryRepository.MediaExistsAsync(file))
                    continue;

                //
                // Create a basic library item.
                // Metadata enrichment will happen later.
                //

                var mediaItem = new DJLMMediaItem
                {
                    Provider = "Discovery",
                    FilePath = file,
                    FileSize = new FileInfo(file).Length,
                    MediaType = MediaFileTypes.IsAudio(file)
                        ? "Audio"
                        : "Video"
                };

                //
                // Save into the DIASISS library.
                //

                await _libraryRepository.AddMediaItemAsync(mediaItem);
            }
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