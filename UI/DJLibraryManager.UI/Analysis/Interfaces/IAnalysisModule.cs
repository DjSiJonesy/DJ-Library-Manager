using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Analysis.Modules;
using DJLibraryManager.UI.Models.Media;

namespace DJLibraryManager.UI.Analysis.Interfaces;

/// <summary>
/// Defines a single analysis module.
/// Each module analyses one track at a time.
/// </summary>
public interface IAnalysisModule
{
    /// <summary>
    /// Display name of the analysis module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Called before analysis starts.
    /// </summary>
    void Begin();

    /// <summary>
    /// Analyses a single media item.
    /// </summary>
    void Analyse(DJLMMediaItem mediaItem);

    /// <summary>
    /// Called after all tracks have been analysed and returns the result.
    /// </summary>
    AnalysisCategoryResult Complete();
}