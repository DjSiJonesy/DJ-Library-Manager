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

    /// <summary>
    /// Raised as each track is analysed.
    /// </summary>
    public event EventHandler<AnalysisProgressEventArgs>? ProgressChanged;

    public AnalysisEngine(
        IEnumerable<IAnalysisModule> modules)
    {
        _modules =
            modules.ToList();
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

        var totalTracks =
            mediaItems.Count;

        var tracksScanned = 0;

        foreach (var mediaItem in mediaItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var module in _modules)
            {
                module.Analyse(mediaItem);
            }

            tracksScanned++;

            var progress =
                totalTracks == 0
                    ? 100
                    : (double)tracksScanned /
                      totalTracks *
                      100.0;

            ProgressChanged?.Invoke(
                this,
                new AnalysisProgressEventArgs(
                    tracksScanned,
                    totalTracks,
                    progress,
                    GetTrackDescription(
                        mediaItem)));
        }

        var categories =
            _modules
                .Select(m => m.Complete())
                .ToList();

        return Task.FromResult(
            new LibraryAnalysisResult
            {
                AnalysisDate =
                    DateTime.Now,

                TracksScanned =
                    tracksScanned,

                TotalTracks =
                    totalTracks,

                HealthScore =
                    CalculateHealth(
                        categories),

                Categories =
                    categories
            });
    }

    private static string GetTrackDescription(
        DJLMMediaItem mediaItem)
    {
        var artist =
            mediaItem.Artist?.Trim();

        var title =
            mediaItem.Title?.Trim();

        if (!string.IsNullOrWhiteSpace(artist) &&
            !string.IsNullOrWhiteSpace(title))
        {
            return $"{artist} - {title}";
        }

        if (!string.IsNullOrWhiteSpace(title))
            return title;

        if (!string.IsNullOrWhiteSpace(artist))
            return artist;

        return mediaItem.FilePath ??
               "Unknown track";
    }

    private static double CalculateHealth(
        IReadOnlyCollection<AnalysisCategoryResult> categories)
    {
        if (categories.Count == 0)
            return 100;

        return Math.Round(
            categories.Average(
                c => c.HealthScore),
            1);
    }
}

/// <summary>
/// Provides progress information while the library is being analysed.
/// </summary>
public sealed class AnalysisProgressEventArgs : EventArgs
{
    public int TracksScanned { get; }

    public int TotalTracks { get; }

    public double Progress { get; }

    public string CurrentTrack { get; }

    public AnalysisProgressEventArgs(
        int tracksScanned,
        int totalTracks,
        double progress,
        string currentTrack)
    {
        TracksScanned =
            tracksScanned;

        TotalTracks =
            totalTracks;

        Progress =
            progress;

        CurrentTrack =
            currentTrack;
    }
}