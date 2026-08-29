using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Provides a live overview of the entire DIASISS DJ library.
///
/// Displayed permanently within the left navigation panel.
///
/// Current library statistics are supplied by
/// LibraryStatisticsService.
///
/// Analysis-specific values such as Health, Missing,
/// Duplicates and Metadata issues are supplied by
/// the AnalysisRepository.
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

        _ = RefreshAsync();
    }

    // ============================================================
    // Library Statistics
    // ============================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DriveCountDisplay))]
    private int driveCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FolderCountDisplay))]
    private int folderCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalFileCountDisplay))]
    private int totalFileCount;

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
    [NotifyPropertyChangedFor(nameof(MetadataIssueCountDisplay))]
    private int metadataIssueCount;

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

    public string TotalFileCountDisplay =>
        TotalFileCount.ToString("N0");

    public string AudioFileCountDisplay =>
        AudioFileCount.ToString("N0");

    public string VideoFileCountDisplay =>
        VideoFileCount.ToString("N0");

    public string MissingFileCountDisplay =>
        MissingFileCount.ToString("N0");

    public string DuplicateFileCountDisplay =>
        DuplicateFileCount.ToString("N0");

    public string MetadataIssueCountDisplay =>
        MetadataIssueCount.ToString("N0");

    // ============================================================
    // Application State
    // ============================================================

    private void ApplicationState_Changed(
        object? sender,
        EventArgs e)
    {
        _ = RefreshAsync();
    }

    // ============================================================
    // Refresh
    // ============================================================

    /// <summary>
    /// Refreshes the Library Overview from the current
    /// DIASISS library statistics and analysis results.
    /// </summary>
    public void Refresh()
    {
        _ = RefreshAsync();
    }

    /// <summary>
    /// Asynchronously refreshes the Library Overview.
    ///
    /// Library statistics come from LibraryStatisticsService,
    /// which uses the authoritative DIASISS SQLite library.
    ///
    /// Analysis values come from the current analysis result.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            // ----------------------------------------------------
            // Get authoritative library statistics.
            // ----------------------------------------------------

            var statistics =
                await App.Services
                    .LibraryStatisticsService
                    .GetStatisticsAsync();

            // ----------------------------------------------------
            // Get current analysis result.
            // ----------------------------------------------------

            var analysis =
                App.Services
                    .AnalysisRepository
                    .CurrentAnalysis;

            // ----------------------------------------------------
            // Update the UI on the Avalonia UI thread.
            // ----------------------------------------------------

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // =================================================
                // Library Statistics
                // =================================================

                DriveCount =
                    statistics.DriveCount;

                FolderCount =
                    statistics.FolderCount;

                TotalFileCount =
                    statistics.LibraryTrackCount;

                AudioFileCount =
                    statistics.AudioFileCount;

                VideoFileCount =
                    statistics.VideoFileCount;

                TotalSize =
                    FormatSize(
                        statistics.TotalSizeBytes);

                // =================================================
                // Analysis Statistics
                // =================================================

                if (analysis is null)
                {
                    MissingFileCount = 0;
                    DuplicateFileCount = 0;
                    MetadataIssueCount = 0;
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

                MetadataIssueCount =
                    GetCategoryIssueCount(
                        analysis,
                        "Metadata");

                HealthScore =
                    (int)Math.Round(
                        analysis.HealthScore,
                        MidpointRounding.AwayFromZero);
            });
        }
        catch
        {
            // ----------------------------------------------------
            // The Overview must never bring down the application
            // because a statistics refresh failed.
            //
            // The existing values are retained.
            // ----------------------------------------------------
        }
    }

    // ============================================================
    // Analysis Data
    // ============================================================

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

    private static string FormatSize(
        long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;
        const double tb = gb * 1024;

        if (bytes >= tb)
            return $"{bytes / tb:N2} TB";

        if (bytes >= gb)
            return $"{bytes / gb:N2} GB";

        if (bytes >= mb)
            return $"{bytes / mb:N2} MB";

        if (bytes >= kb)
            return $"{bytes / kb:N2} KB";

        return $"{bytes:N0} Bytes";
    }
}