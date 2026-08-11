using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Analysis.Interfaces;

/// <summary>
/// Defines a single analysis module.
/// </summary>
public interface IAnalysisModule
{
    /// <summary>
    /// Display name of the analysis module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Analyses the supplied media library.
    /// </summary>
    Task<AnalysisCategoryResult> AnalyseAsync(
        IReadOnlyList<DJLMMediaItem> mediaItems,
        CancellationToken cancellationToken = default);
}