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
    /// Runs all analysis modules and returns the combined results.
    /// </summary>
    public async Task<LibraryAnalysisResult> AnalyseAsync(
    IReadOnlyList<DJLMMediaItem> mediaItems,
    CancellationToken cancellationToken = default)
    {
        var categories = new List<AnalysisCategoryResult>();

        foreach (var module in _modules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await module.AnalyseAsync(mediaItems, cancellationToken);

            categories.Add(result);
        }

        return new LibraryAnalysisResult
        {
            AnalysisDate = DateTime.Now,
            Categories = categories,
            TracksScanned = mediaItems.Count,
            HealthScore = CalculateHealth(categories)
        };
    }

    private static double CalculateHealth(
        IReadOnlyCollection<AnalysisCategoryResult> categories)
    {
        if (categories.Count == 0)
            return 100;

        return categories.Average(c => c.HealthScore);
    }
}