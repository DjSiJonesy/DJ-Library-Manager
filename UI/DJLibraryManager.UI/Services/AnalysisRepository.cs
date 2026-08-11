using System.IO;
using System.Text.Json;
using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Analysis.Models;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Stores and retrieves the latest Library Analysis.
/// </summary>
public sealed class AnalysisRepository
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    public AnalysisRepository()
    {
        Load();
    }

    /// <summary>
    /// The latest completed analysis.
    /// </summary>
    public LibraryAnalysisResult? CurrentAnalysis { get; private set; }

    /// <summary>
    /// Returns true if an analysis exists.
    /// </summary>
    public bool HasAnalysis =>
        CurrentAnalysis is not null;

    /// <summary>
    /// Saves the latest analysis.
    /// </summary>
    public void Save(LibraryAnalysisResult result)
    {
        CurrentAnalysis = result;

        Directory.CreateDirectory(ApplicationPaths.Analysis);

        var document = new AnalysisDocument
        {
            Version = 1,
            Analysis = result
        };

        File.WriteAllText(
            ApplicationPaths.LatestAnalysis,
            JsonSerializer.Serialize(document, JsonOptions));
    }

    /// <summary>
    /// Loads the latest analysis from disk.
    /// </summary>
    private void Load()
    {
        if (!File.Exists(ApplicationPaths.LatestAnalysis))
            return;

        var json = File.ReadAllText(ApplicationPaths.LatestAnalysis);

        var document =
            JsonSerializer.Deserialize<AnalysisDocument>(json);

        if (document is null)
            return;

        //
        // Future compatibility.
        //
        switch (document.Version)
        {
            case 1:
                CurrentAnalysis = document.Analysis;
                break;

            default:
                // Unknown version.
                CurrentAnalysis = null;
                break;
        }
    }

    /// <summary>
    /// Clears the current analysis.
    /// </summary>
    public void Clear()
    {
        CurrentAnalysis = null;

        if (File.Exists(ApplicationPaths.LatestAnalysis))
        {
            File.Delete(ApplicationPaths.LatestAnalysis);
        }
    }
}