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
    // Selected Category
    // ============================================================

    [ObservableProperty]
    private AnalysisCategoryInfo? selectedCategory;

    // ============================================================
    // Selected Category Visibility
    // ============================================================

    /// <summary>
    /// Indicates whether Metadata is currently selected.
    /// </summary>
    public bool IsMetadataSelected =>
        string.Equals(
            SelectedCategory?.Name,
            "Metadata",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indicates whether File Integrity is currently selected.
    /// </summary>
    public bool IsFileIntegritySelected =>
        string.Equals(
            SelectedCategory?.Name,
            "File Integrity",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indicates whether Duplicates is currently selected.
    /// </summary>
    public bool IsDuplicatesSelected =>
        string.Equals(
            SelectedCategory?.Name,
            "Duplicates",
            StringComparison.OrdinalIgnoreCase);

    // ============================================================
    // Selected Category Detail
    // ============================================================

    /// <summary>
    /// Number of selected-category issues where Artist is missing.
    /// </summary>
    public int MissingArtistCount =>
        CountMissingField("Artist");

    /// <summary>
    /// Number of selected-category issues where Title is missing.
    /// </summary>
    public int MissingTitleCount =>
        CountMissingField("Title");

    /// <summary>
    /// Number of selected-category issues where Album is missing.
    /// </summary>
    public int MissingAlbumCount =>
        CountMissingField("Album");

    /// <summary>
    /// Number of selected-category issues where Genre is missing.
    /// </summary>
    public int MissingGenreCount =>
        CountMissingField("Genre");

    /// <summary>
    /// Number of selected-category issues where Year is missing.
    /// </summary>
    public int MissingYearCount =>
        CountMissingField("Year");

    /// <summary>
    /// Number of selected-category issues where BPM is missing.
    /// </summary>
    public int MissingBPMCount =>
        CountMissingField("BPM");

    /// <summary>
    /// Number of selected-category issues where Musical Key is missing.
    /// </summary>
    public int MissingKeyCount =>
        CountMissingField("Key");

    /// <summary>
    /// Number of selected-category issues where Duration is missing.
    /// </summary>
    public int MissingDurationCount =>
        CountMissingField("Duration");

    /// <summary>
    /// Number of issues in the currently selected category.
    /// </summary>
    public int SelectedCategoryIssueCount =>
        SelectedCategory?.IssueCount ?? 0;

    /// <summary>
    /// Health score of the currently selected category.
    /// </summary>
    public double SelectedCategoryHealthScore =>
        SelectedCategory?.HealthScore ?? 100;

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
    // Selected Category Changed
    // ============================================================

    partial void OnSelectedCategoryChanged(
    AnalysisCategoryInfo? value)
    {
        OnPropertyChanged(nameof(MissingArtistCount));
        OnPropertyChanged(nameof(MissingTitleCount));
        OnPropertyChanged(nameof(MissingAlbumCount));
        OnPropertyChanged(nameof(MissingGenreCount));
        OnPropertyChanged(nameof(MissingYearCount));
        OnPropertyChanged(nameof(MissingBPMCount));
        OnPropertyChanged(nameof(MissingKeyCount));
        OnPropertyChanged(nameof(MissingDurationCount));

        OnPropertyChanged(nameof(SelectedCategoryIssueCount));
        OnPropertyChanged(nameof(SelectedCategoryHealthScore));

        OnPropertyChanged(nameof(IsMetadataSelected));
        OnPropertyChanged(nameof(IsFileIntegritySelected));
        OnPropertyChanged(nameof(IsDuplicatesSelected));
    }

    // ============================================================
    // Initialisation
    // ============================================================

    private async Task InitializeAsync()
    {
        var statistics =
            await App.Services
                .LibraryStatisticsService
                .GetStatisticsAsync();

        TotalTracks =
            statistics.LibraryTrackCount;

        var previousAnalysis =
            App.Services
                .AnalysisRepository
                .CurrentAnalysis;

        if (previousAnalysis is null)
            return;

        TracksScanned =
            previousAnalysis.TracksScanned;

        IssuesFound =
            previousAnalysis.TotalIssues;

        HealthScore =
            previousAnalysis.HealthScore;

        LastAnalysed =
            previousAnalysis.AnalysisDate;

        Progress = 100;

        Status = "Complete";
        StatusBrush = Brushes.LimeGreen;

        CurrentTrack =
            $"{TracksScanned:N0} tracks analysed";

        Categories.Clear();

        foreach (var category in
         previousAnalysis.Categories
             .OrderBy(x => x.Name))
        {
            Categories.Add(new AnalysisCategoryInfo
            {
                Name = category.Name,
                Description = GetCategoryDescription(category.Name),
                IssueCount = category.IssueCount,
                HealthScore = category.HealthScore,
                Issues = category.Issues
            });
        }

        SelectedCategory =
            Categories.FirstOrDefault();
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

        SelectedCategory = null;

        // --------------------------------------------------------
        // Metadata
        // --------------------------------------------------------

        Categories.Add(
            new AnalysisCategoryInfo
            {
                Name = "Metadata",

                Description =
                    GetCategoryDescription(
                        "Metadata"),

                HealthScore = 100
            });

        // --------------------------------------------------------
        // File Integrity
        // --------------------------------------------------------

        Categories.Add(
            new AnalysisCategoryInfo
            {
                Name = "File Integrity",

                Description =
                    GetCategoryDescription(
                        "File Integrity"),

                HealthScore = 100
            });

        // --------------------------------------------------------
        // Duplicates
        // --------------------------------------------------------

        Categories.Add(
            new AnalysisCategoryInfo
            {
                Name = "Duplicates",

                Description =
                    GetCategoryDescription(
                        "Duplicates"),

                HealthScore = 100
            });
    }

    // ============================================================
    // Category Descriptions
    // ============================================================

    private static string GetCategoryDescription(
        string categoryName)
    {
        return categoryName switch
        {
            "Metadata" =>
                "Checks the required information associated with each track, including Artist, Title, Album, Genre, Year, BPM, Key and Duration.",

            "File Integrity" =>
                "Checks whether library files exist and can be accessed.",

            "Duplicates" =>
                "Identifies groups of tracks that appear to be duplicates.",

            _ =>
                string.Empty
        };
    }

    // ============================================================
    // Metadata Detail Counts
    // ============================================================

    private int CountMissingField(
        string fieldName)
    {
        if (SelectedCategory?.Issues is null)
            return 0;

        return SelectedCategory.Issues.Count(
            issue =>
                issue.MissingFields.Any(
                    field =>
                        string.Equals(
                            field,
                            fieldName,
                            StringComparison.OrdinalIgnoreCase)));
    }

    // ============================================================
    // Analysis
    // ============================================================

    [RelayCommand]
    private async Task AnalyseLibrary()
    {
        Status = "Running";
        StatusBrush = Brushes.Goldenrod;

        CurrentTrack =
            "Loading Library...";

        Progress = 0;

        TracksScanned = 0;

        try
        {
            var result =
                await App.Services
                    .Analysis
                    .AnalyseLibraryAsync();

            App.Services
                .AnalysisRepository
                .Save(result);

            TracksScanned =
                result.TracksScanned;

            IssuesFound =
                result.TotalIssues;

            HealthScore =
                result.HealthScore;

            LastAnalysed =
                result.AnalysisDate;

            Progress = 100;

            Status = "Complete";
            StatusBrush = Brushes.LimeGreen;

            CurrentTrack =
                $"{TracksScanned:N0} tracks analysed";

            Categories.Clear();

            foreach (var category in
         result.Categories
             .OrderBy(c => c.Name))
            {
                Categories.Add(new AnalysisCategoryInfo
                {
                    Name = category.Name,
                    Description = GetCategoryDescription(category.Name),
                    IssueCount = category.IssueCount,
                    HealthScore = category.HealthScore,
                    Issues = category.Issues
                });
            }

            SelectedCategory =
                Categories.FirstOrDefault();

            App.Services
                .ApplicationState
                .NotifyAnalysisCompleted();
        }
        catch
        {
            Status = "Error";

            StatusBrush =
                Brushes.Red;

            CurrentTrack =
                "Analysis failed";

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
        TracksScanned =
            e.TracksScanned;

        TotalTracks =
            e.TotalTracks;

        Progress =
            e.Progress;

        CurrentTrack =
            e.CurrentTrack;
    }

    // ============================================================
    // Category Selection
    // ============================================================

    [RelayCommand]
    private void SelectCategory(
        AnalysisCategoryInfo? category)
    {
        if (category is null)
            return;

        SelectedCategory = category;
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
        App.Services
            .ApplicationState
            .NavigateTo(
                WorkspaceType.Import);
    }

    /// <summary>
    /// Moves to the Search workflow.
    /// </summary>
    [RelayCommand]
    private void Next()
    {
        App.Services
            .ApplicationState
            .NavigateTo(
                WorkspaceType.Search);
    }
}