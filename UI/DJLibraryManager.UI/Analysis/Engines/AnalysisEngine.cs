using DJLibraryManager.UI.Analysis.Interfaces;
using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Analysis.Engines;

/// <summary>
/// Coordinates execution of all registered analysis modules.
/// </summary>
public sealed class AnalysisEngine
{
    private readonly IReadOnlyList<IAnalysisModule> _modules;

    public AnalysisEngine(IEnumerable<IAnalysisModule> modules)
    {
        _modules = modules.ToList();
    }

    /// <summary>
    /// Runs all analysis modules over the library.
    /// </summary>
    public Task<LibraryAnalysisResult> AnalyseAsync(
        IReadOnlyList<DJLMMediaItem> mediaItems,
        CancellationToken cancellationToken = default)
    {
        foreach (var module in _modules)
        {
            module.Begin();
        }

        foreach (var mediaItem in mediaItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var module in _modules)
            {
                module.Analyse(mediaItem);
            }
        }

        var categories = _modules
            .Select(m => m.Complete())
            .ToList();

        return Task.FromResult(
            new LibraryAnalysisResult
            {
                AnalysisDate = DateTime.Now,
                TracksScanned = mediaItems.Count,
                HealthScore = CalculateHealth(categories),
                Categories = categories
            });
    }

    private static double CalculateHealth(
        IReadOnlyCollection<AnalysisCategoryResult> categories)
    {
        if (categories.Count == 0)
            return 100;

        return categories.Average(c => c.HealthScore);
    }
}