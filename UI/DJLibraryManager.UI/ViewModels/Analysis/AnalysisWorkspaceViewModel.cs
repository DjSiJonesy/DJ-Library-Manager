using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.UI.ViewModels.Workspace;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Analysis;

/// <summary>
/// ViewModel for the Analysis workspace.
/// </summary>
public partial class AnalysisWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title => "Analysis";

    public override string Subtitle =>
        "Analyse the DIASISS library and identify issues before searching for improvements.";

    [ObservableProperty]
    private int tracksScanned;

    [ObservableProperty]
    private int issuesFound;

    [ObservableProperty]
    private double healthScore = 100;

    [ObservableProperty]
    private string lastAnalysed = "Never";

    public AnalysisWorkspaceViewModel()
    {
        // Placeholder values until the Analysis Service is connected.
        TracksScanned = 0;
        IssuesFound = 0;
        HealthScore = 100;
        LastAnalysed = "Never";
    }

    /// <summary>
    /// Starts a library analysis.
    /// </summary>
    [RelayCommand]
    private async Task AnalyseLibrary()
    {
        // This will call App.Services.Analysis.AnalyseLibraryAsync()
        // once the workspace UI has been completed.
        await Task.CompletedTask;
    }
}