using DJLibraryManager.Core.Services;
using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Analysis.Engines;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Analysis.Modules;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Executes a complete library analysis.
///
/// Analysis determines the current health of the DIASISS library.
/// It does not search external sources and does not modify the
/// library.
///
/// Files physically located inside the DIASISS Duplicates folder
/// are deliberately excluded from Analysis. These files have been
/// moved there by Improve as part of duplicate protection and
/// remain available for future recovery/Undo.
/// </summary>
public sealed class AnalysisService
{
    private readonly LibraryRepository _libraryRepository;

    /// <summary>
    /// Raised as each track is analysed.
    /// </summary>
    public event EventHandler<AnalysisProgressEventArgs>? ProgressChanged;

    public AnalysisService(
        LibraryRepository libraryRepository)
    {
        _libraryRepository =
            libraryRepository
            ?? throw new ArgumentNullException(
                nameof(libraryRepository));
    }

    /// <summary>
    /// Analyses the current DIASISS library.
    ///
    /// Files inside the DIASISS Duplicates folder are excluded
    /// before the Analysis Engine receives the library collection.
    /// </summary>
    public async Task<LibraryAnalysisResult> AnalyseLibraryAsync(
        CancellationToken cancellationToken = default)
    {
        var mediaItems =
            await _libraryRepository.LoadAsync();

        cancellationToken.ThrowIfCancellationRequested();

        // --------------------------------------------------------
        // DIASISS Duplicates Protection
        //
        // Files moved into DIASISS Duplicates by Improve are
        // retained physically and remain recorded in the library
        // for recovery purposes.
        //
        // They must not, however, participate in the normal
        // library health analysis.
        // --------------------------------------------------------

        var analysisMediaItems =
            mediaItems
                .Where(
                    media =>
                        !IsInsideDiasissDuplicatesFolder(
                            media.FilePath))
                .ToList();

        var engine =
            new AnalysisEngine(
            [
                // =================================================
                // Metadata
                // =================================================
                //
                // Checks completeness of all required track
                // metadata including Artist, Title, Album, Genre,
                // Year, BPM, Key and Duration.
                //
                new MetadataAnalysisModule(),

                // =================================================
                // File Integrity
                // =================================================
                //
                // Checks whether files recorded in the library
                // are actually available.
                //
                new FileIntegrityAnalysisModule(),

                // =================================================
                // Duplicates
                // =================================================
                //
                // Identifies duplicate groups within the active
                // library.
                //
                new DuplicateAnalysisModule()
            ]);

        engine.ProgressChanged +=
            Engine_ProgressChanged;

        try
        {
            return await Task.Run(
                () =>
                    engine.AnalyseAsync(
                        analysisMediaItems,
                        cancellationToken),
                cancellationToken);
        }
        finally
        {
            engine.ProgressChanged -=
                Engine_ProgressChanged;
        }
    }

    // ============================================================
    // DIASISS Duplicates Detection
    // ============================================================

    /// <summary>
    /// Determines whether a media file is physically located
    /// inside the DIASISS Duplicates folder.
    ///
    /// The comparison includes the complete directory path so
    /// that similarly named folders are not incorrectly excluded.
    ///
    /// Examples:
    ///
    /// C:\Users\...\Music\DIASISS Duplicates\Track.mp3
    ///     -> excluded
    ///
    /// C:\Users\...\Music\DIASISS Duplicates\SubFolder\Track.mp3
    ///     -> excluded
    ///
    /// C:\Users\...\Music\DIASISS Duplicates Old\Track.mp3
    ///     -> not excluded
    /// </summary>
    private static bool IsInsideDiasissDuplicatesFolder(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var duplicatesRoot =
            ApplicationPaths.DiasissDuplicates;

        if (string.IsNullOrWhiteSpace(duplicatesRoot))
            return false;

        try
        {
            var fullFilePath =
                Path.GetFullPath(filePath);

            var fullDuplicatesRoot =
                Path.GetFullPath(duplicatesRoot)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

            var duplicatesPrefix =
                fullDuplicatesRoot +
                Path.DirectorySeparatorChar;

            return
                string.Equals(
                    fullFilePath,
                    fullDuplicatesRoot,
                    StringComparison.OrdinalIgnoreCase)
                ||
                fullFilePath.StartsWith(
                    duplicatesPrefix,
                    StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // If the path cannot be safely evaluated, do not
            // exclude it from Analysis.
            return false;
        }
    }

    // ============================================================
    // Progress
    // ============================================================

    private void Engine_ProgressChanged(
        object? sender,
        AnalysisProgressEventArgs e)
    {
        ProgressChanged?.Invoke(
            this,
            e);
    }
}