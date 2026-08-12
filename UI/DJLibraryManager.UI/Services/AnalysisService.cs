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
        _libraryRepository = libraryRepository;
    }

    /// <summary>
    /// Analyses the current DIASISS library.
    /// </summary>
    public async Task<LibraryAnalysisResult> AnalyseLibraryAsync(
        CancellationToken cancellationToken = default)
    {
        var mediaItems = await _libraryRepository.LoadAsync();

        var engine = new AnalysisEngine(
        [
            new MetadataAnalysisModule(),
            new FileIntegrityAnalysisModule(),
            new DuplicateAnalysisModule(),
            new MusicAnalysisModule(),
            new ProviderAnalysisModule()
        ]);

        engine.ProgressChanged += Engine_ProgressChanged;

        try
        {
            return await Task.Run(
                () => engine.AnalyseAsync(
                    mediaItems,
                    cancellationToken),
                cancellationToken);
        }
        finally
        {
            engine.ProgressChanged -= Engine_ProgressChanged;
        }
    }

    private void Engine_ProgressChanged(
        object? sender,
        AnalysisProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }
}