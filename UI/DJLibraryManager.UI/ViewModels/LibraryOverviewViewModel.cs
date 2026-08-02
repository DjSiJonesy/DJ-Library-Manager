using CommunityToolkit.Mvvm.ComponentModel;
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
        Refresh();
    }

    [ObservableProperty]
    private int driveCount;

    [ObservableProperty]
    private int folderCount;

    [ObservableProperty]
    private int audioFileCount;

    [ObservableProperty]
    private int videoFileCount;

    [ObservableProperty]
    private int missingFileCount;

    [ObservableProperty]
    private int duplicateFileCount;

    [ObservableProperty]
    private string totalSize = "0 Bytes";

    [ObservableProperty]
    private int healthScore = 100;

    /// <summary>
    /// Refreshes the library overview.
    /// Initially populated from discovery.
    /// Analysis metrics will be added in the next sprint.
    /// </summary>
    public void Refresh()
    {
        var repository = App.Services.DiscoveryRepository;

        var sessions = repository.DiscoverySessions;

        DriveCount = sessions.Count;

        FolderCount = sessions.Sum(x => x.FolderCount);

        AudioFileCount = sessions.Sum(x => x.AudioFileCount);

        VideoFileCount = sessions.Sum(x => x.VideoFileCount);

        TotalSize = FormatSize(
            sessions.Sum(x => x.TotalSizeBytes));

        //
        // Placeholder until Analysis Workspace exists.
        //

        MissingFileCount = 0;

        DuplicateFileCount = 0;

        HealthScore = 100;
    }

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