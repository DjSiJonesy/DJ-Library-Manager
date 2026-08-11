using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string currentStage = "Ready";

    [ObservableProperty]
    private string currentTrack = "—";

    public ObservableCollection<AnalysisCategoryInfo> Categories { get; } = new();

    public AnalysisWorkspaceViewModel()
    {
        Reset();
        _ = InitializeAsync();
    }

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
        CurrentStage = "Complete";
        CurrentTrack = $"{TracksScanned:N0} tracks analysed";

        Categories.Clear();

        foreach (var category in previousAnalysis.Categories.OrderBy(x => x.Name))
        {
            Categories.Add(new AnalysisCategoryInfo
            {
                Name = category.Name,
                IssueCount = category.IssueCount,
                HealthScore = category.HealthScore
            });
        }
    }

    private void Reset()
    {
        TracksScanned = 0;
        TotalTracks = 0;
        IssuesFound = 0;
        HealthScore = 100;
        Progress = 0;
        CurrentStage = "Ready";
        CurrentTrack = "—";
        LastAnalysed = null;

        Categories.Clear();

        Categories.Add(new AnalysisCategoryInfo { Name = "Metadata", HealthScore = 100 });
        Categories.Add(new AnalysisCategoryInfo { Name = "File Integrity", HealthScore = 100 });
        Categories.Add(new AnalysisCategoryInfo { Name = "Duplicates", HealthScore = 100 });
        Categories.Add(new AnalysisCategoryInfo { Name = "Music", HealthScore = 100 });
        Categories.Add(new AnalysisCategoryInfo { Name = "Providers", HealthScore = 100 });
    }

    [RelayCommand]
    private async Task AnalyseLibrary()
    {
        CurrentStage = "Analysing Library";
        CurrentTrack = "Loading Library...";
        Progress = 0;

        var result = await App.Services.Analysis.AnalyseLibraryAsync();
        App.Services.AnalysisRepository.Save(result);

        TracksScanned = result.TracksScanned;
        IssuesFound = result.TotalIssues;
        HealthScore = result.HealthScore;
        LastAnalysed = result.AnalysisDate;

        Progress = 100;

        CurrentStage = "Complete";
        CurrentTrack = $"{TracksScanned:N0} tracks analysed";

        Categories.Clear();

        foreach (var category in result.Categories.OrderBy(c => c.Name))
        {
            Categories.Add(new AnalysisCategoryInfo
            {
                Name = category.Name,
                IssueCount = category.IssueCount,
                HealthScore = category.HealthScore
            });
        }

        App.Services.ApplicationState.NotifyAnalysisCompleted();
    }
}