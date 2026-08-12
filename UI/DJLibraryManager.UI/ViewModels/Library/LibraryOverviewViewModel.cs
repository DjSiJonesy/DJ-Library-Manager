using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.Analysis.Models;
using System;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Provides a live overview of the entire DIASISS DJ library.
/// Displayed permanently within the left navigation panel.
/// </summary>
public partial class LibraryOverviewViewModel : ViewModelBase
{
    public LibraryOverviewViewModel()
    {
        App.Services.ApplicationState.DiscoveryChanged +=
            ApplicationState_Changed;

        App.Services.ApplicationState.LibraryImported +=
            ApplicationState_Changed;

        App.Services.ApplicationState.AnalysisCompleted +=
            ApplicationState_Changed;

        Refresh();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DriveCountDisplay))]
    private int driveCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderCountDisplay))]
    private int folderCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioFileCountDisplay))]
    private int audioFileCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoFileCountDisplay))]
    private int videoFileCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MissingFileCountDisplay))]
    private int missingFileCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DuplicateFileCountDisplay))]
    private int duplicateFileCount;

    [ObservableProperty]
    private string totalSize = "0 Bytes";

    [ObservableProperty]
    private int healthScore = 100;

    // ============================================================
    // Display Properties
    // ============================================================

    public string DriveCountDisplay =>
        DriveCount.ToString("N0");

    public string FolderCountDisplay =>
        FolderCount.ToString("N0");

    public string AudioFileCountDisplay =>
        AudioFileCount.ToString("N0");

    public string VideoFileCountDisplay =>
        VideoFileCount.ToString("N0");

    public string MissingFileCountDisplay =>
        MissingFileCount.ToString("N0");

    public string DuplicateFileCountDisplay =>
        DuplicateFileCount.ToString("N0");

    // ============================================================
    // Application State
    // ============================================================

    private void ApplicationState_Changed(
        object? sender,
        EventArgs e)
    {
        Refresh();
    }

    // ============================================================
    // Refresh
    // ============================================================

    /// <summary>
    /// Refreshes the library overview from the current
    /// discovery and analysis data.
    /// </summary>
    public void Refresh()
    {
        RefreshDiscoveryData();
        RefreshAnalysisData();
    }

    // ============================================================
    // Discovery Data
    // ============================================================

    private void RefreshDiscoveryData()
    {
        var repository =
            App.Services.DiscoveryRepository;

        var sessions =
            repository.DiscoverySessions;

        DriveCount =
            sessions.Count;

        FolderCount =
            sessions.Sum(x => x.FolderCount);

        AudioFileCount =
            sessions.Sum(x => x.AudioFileCount);

        VideoFileCount =
            sessions.Sum(x => x.VideoFileCount);

        TotalSize =
            FormatSize(
                sessions.Sum(x => x.TotalSizeBytes));
    }

    // ============================================================
    // Analysis Data
    // ============================================================

    private void RefreshAnalysisData()
    {
        var analysis =
            App.Services.AnalysisRepository.CurrentAnalysis;

        if (analysis is null)
        {
            MissingFileCount = 0;
            DuplicateFileCount = 0;
            HealthScore = 100;

            return;
        }

        MissingFileCount =
            GetCategoryIssueCount(
                analysis,
                "File Integrity");

        DuplicateFileCount =
            GetCategoryIssueCount(
                analysis,
                "Duplicates");

        HealthScore =
            (int)Math.Round(
                analysis.HealthScore,
                MidpointRounding.AwayFromZero);
    }

    private static int GetCategoryIssueCount(
        LibraryAnalysisResult analysis,
        string categoryName)
    {
        var category =
            analysis.Categories.FirstOrDefault(
                x => x.Name.Equals(
                    categoryName,
                    StringComparison.OrdinalIgnoreCase));

        return category?.IssueCount ?? 0;
    }

    // ============================================================
    // Formatting
    // ============================================================

    private static string FormatSize(long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        if (bytes >= gb)
            return $"{bytes / gb:N2} GB";

        if (bytes >= mb)
            return $"{bytes / mb:N2} MB";

        if (bytes >= kb)
            return $"{bytes / kb:N2} KB";

        return $"{bytes:N0} Bytes";
    }
}