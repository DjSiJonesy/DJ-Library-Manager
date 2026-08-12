using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Analysis.Engines;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Analysis;
using DJLibraryManager.UI.ViewModels.Workspace;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Analysis;

public partial class AnalysisWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title => "Analysis";

    public override string Subtitle =>
        "Analyse the DIASISS library and identify issues before searching for improvements.";

    // ============================================================
    // Summary
    // ============================================================

    [ObservableProperty]
    private int tracksScanned;

    [ObservableProperty]
    private int totalTracks;

    [ObservableProperty]
    private int issuesFound;

    [ObservableProperty]
    private double healthScore = 100;

    [ObservableProperty]
    private DateTime? lastAnalysed;

    // ============================================================
    // Progress
    // ============================================================

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    private IBrush statusBrush = Brushes.Gray;

    [ObservableProperty]
    private string currentTrack = "—";

    // ============================================================
    // Categories
    // ============================================================

    public ObservableCollection<AnalysisCategoryInfo> Categories { get; }
        = new();

    // ============================================================
    // Constructor
    // ============================================================

    public AnalysisWorkspaceViewModel()
    {
        Reset();

        App.Services.Analysis.ProgressChanged +=
            Analysis_ProgressChanged;

        _ = InitializeAsync();
    }

    // ============================================================
    // Initialisation
    // ============================================================

    private async Task InitializeAsync()
    {
        var statistics = await App.Services
            .LibraryStatisticsService
            .GetStatisticsAsync();

        TotalTracks = statistics.LibraryTrackCount;

        var previousAnalysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (previousAnalysis is null)
            return;

        TracksScanned = previousAnalysis.TracksScanned;
        IssuesFound = previousAnalysis.TotalIssues;
        HealthScore = previousAnalysis.HealthScore;
        LastAnalysed = previousAnalysis.AnalysisDate;

        Progress = 100;

        Status = "Complete";
        StatusBrush = Brushes.LimeGreen;

        CurrentTrack = $"{TracksScanned:N0} tracks analysed";

        Categories.Clear();

        foreach (var category in previousAnalysis.Categories.OrderBy(x => x.Name))
        {
            Categories.Add(new AnalysisCategoryInfo
            {
                Name = category.Name,
                Description = GetCategoryDescription(category.Name),
                IssueCount = category.IssueCount,
                HealthScore = category.HealthScore
            });
        }
    }

    // ============================================================
    // Reset
    // ============================================================

    private void Reset()
    {
        TracksScanned = 0;
        TotalTracks = 0;
        IssuesFound = 0;

        HealthScore = 100;

        Progress = 0;

        Status = "Ready";
        StatusBrush = Brushes.Gray;

        CurrentTrack = "—";

        LastAnalysed = null;

        Categories.Clear();

        Categories.Add(new AnalysisCategoryInfo
        {
            Name = "Metadata",
            Description = GetCategoryDescription("Metadata"),
            HealthScore = 100
        });

        Categories.Add(new AnalysisCategoryInfo
        {
            Name = "File Integrity",
            Description = GetCategoryDescription("File Integrity"),
            HealthScore = 100
        });

        Categories.Add(new AnalysisCategoryInfo
        {
            Name = "Duplicates",
            Description = GetCategoryDescription("Duplicates"),
            HealthScore = 100
        });

        Categories.Add(new AnalysisCategoryInfo
        {
            Name = "Music",
            Description = GetCategoryDescription("Music"),
            HealthScore = 100
        });

        Categories.Add(new AnalysisCategoryInfo
        {
            Name = "Providers",
            Description = GetCategoryDescription("Providers"),
            HealthScore = 100
        });
    }

    // ============================================================
    // Category Descriptions
    // ============================================================

    private static string GetCategoryDescription(string categoryName)
    {
        return categoryName switch
        {
            "Metadata" =>
                "Checks the descriptive information associated with each track.",

            "File Integrity" =>
                "Checks whether library files exist and can be accessed.",

            "Duplicates" =>
                "Identifies tracks that appear to be duplicates.",

            "Music" =>
                "Checks BPM, musical key and track duration for valid values.",

            "Providers" =>
                "Checks information associated with DJ software provider libraries.",

            _ =>
                string.Empty
        };
    }

    // ============================================================
    // Analysis
    // ============================================================

    [RelayCommand]
    private async Task AnalyseLibrary()
    {
        Status = "Running";
        StatusBrush = Brushes.Goldenrod;

        CurrentTrack = "Loading Library...";
        Progress = 0;
        TracksScanned = 0;

        try
        {
            var result =
                await App.Services.Analysis.AnalyseLibraryAsync();

            App.Services.AnalysisRepository.Save(result);

            TracksScanned = result.TracksScanned;
            IssuesFound = result.TotalIssues;
            HealthScore = result.HealthScore;
            LastAnalysed = result.AnalysisDate;

            Progress = 100;

            Status = "Complete";
            StatusBrush = Brushes.LimeGreen;

            CurrentTrack = $"{TracksScanned:N0} tracks analysed";

            Categories.Clear();

            foreach (var category in result.Categories.OrderBy(c => c.Name))
            {
                Categories.Add(new AnalysisCategoryInfo
                {
                    Name = category.Name,
                    Description = GetCategoryDescription(category.Name),
                    IssueCount = category.IssueCount,
                    HealthScore = category.HealthScore
                });
            }

            App.Services.ApplicationState.NotifyAnalysisCompleted();
        }
        catch
        {
            Status = "Error";
            StatusBrush = Brushes.Red;

            CurrentTrack = "Analysis failed";

            throw;
        }
    }

    // ============================================================
    // Progress
    // ============================================================

    private void Analysis_ProgressChanged(
        object? sender,
        AnalysisProgressEventArgs e)
    {
        TracksScanned = e.TracksScanned;
        TotalTracks = e.TotalTracks;
        Progress = e.Progress;
        CurrentTrack = e.CurrentTrack;
    }

    // ============================================================
    // Workflow Navigation
    // ============================================================

    /// <summary>
    /// Returns to the Import workflow.
    /// </summary>
    [RelayCommand]
    private void Previous()
    {
        App.Services.ApplicationState.NavigateTo(
            WorkspaceType.Import);
    }

    /// <summary>
    /// Moves to the Search workflow.
    ///
    /// Search is not implemented yet, so this command
    /// intentionally does nothing for now.
    /// </summary>
    [RelayCommand]
    private void Next()
    {
        // Search workspace will be connected here
        // when the Search workflow is implemented.
    }
}