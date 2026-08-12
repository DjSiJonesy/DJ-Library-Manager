using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Analysis.Engines;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Analysis.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Executes a complete library analysis.
///
/// Analysis determines the current health of the DIASISS library.
/// It does not search external sources and does not modify the
/// library.
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
    /// </summary>
    public async Task<LibraryAnalysisResult> AnalyseLibraryAsync(
        CancellationToken cancellationToken = default)
    {
        var mediaItems =
            await _libraryRepository.LoadAsync();

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
                // Identifies duplicate groups within the library.
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
                        mediaItems,
                        cancellationToken),
                cancellationToken);
        }
        finally
        {
            engine.ProgressChanged -=
                Engine_ProgressChanged;
        }
    }

    private void Engine_ProgressChanged(
        object? sender,
        AnalysisProgressEventArgs e)
    {
        ProgressChanged?.Invoke(
            this,
            e);
    }
}