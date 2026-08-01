using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;
using DJLibraryManager.UI.Services;

using System.Collections.ObjectModel;
using System.Linq;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Workspace displayed when a media location is selected.
/// </summary>
public partial class MediaLocationWorkspaceViewModel : WorkspaceViewModel
{
    private readonly MediaLibraryDiscoveryService _discoveryService = new();

    public MediaLocation MediaLocation { get; }

    /// <summary>
    /// Libraries discovered beneath this media location.
    /// </summary>
    public ObservableCollection<MediaLibrary> Libraries { get; } = new();

    public override string Title => "Media Location";

    public MediaLocationWorkspaceViewModel(MediaLocation mediaLocation)
    {
        MediaLocation = mediaLocation;

        Status = Exists
            ? "Ready to Discover"
            : "Location Not Available";
    }

    #region Location Information

    public string Name => MediaLocation.Name;

    public string Path => MediaLocation.Path;

    public string Drive => MediaLocation.Drive;

    public bool Exists => MediaLocation.Exists;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    private string status = string.Empty;

    public IBrush StatusBrush =>
        Status switch
        {
            "Discovery Complete" => Brushes.LimeGreen,
            "Discovering..." => Brushes.DeepSkyBlue,
            "Location Not Available" => Brushes.DarkOrange,
            _ => Brushes.Gray
        };

    #endregion

    #region Discovery Summary

    public int FolderCount => Libraries.Count;

    public int AudioFileCount => Libraries.Sum(x => x.AudioFileCount);

    public int VideoFileCount => Libraries.Sum(x => x.VideoFileCount);

    public int TotalMediaFiles => AudioFileCount + VideoFileCount;

    public long TotalSizeBytes => Libraries.Sum(x => x.TotalSizeBytes);

    public string TotalSize
    {
        get
        {
            const double kb = 1024;
            const double mb = kb * 1024;
            const double gb = mb * 1024;

            if (TotalSizeBytes >= gb)
                return $"{TotalSizeBytes / gb:N2} GB";

            if (TotalSizeBytes >= mb)
                return $"{TotalSizeBytes / mb:N2} MB";

            if (TotalSizeBytes >= kb)
                return $"{TotalSizeBytes / kb:N2} KB";

            return $"{TotalSizeBytes:N0} Bytes";
        }
    }

    #endregion

    /// <summary>
    /// Opens the selected media location in Windows Explorer.
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        if (!Exists)
            return;

        FolderLauncher.Open(Path);
    }

    /// <summary>
    /// Discovers media libraries beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private void DiscoverMedia()
    {
        Libraries.Clear();

        if (!Exists)
        {
            Status = "Location Not Available";
            return;
        }

        Status = "Discovering...";

        var libraries = _discoveryService.DiscoverLibraries(MediaLocation);

        foreach (var library in libraries)
        {
            Libraries.Add(library);
        }

        Status = "Discovery Complete";

        OnPropertyChanged(nameof(FolderCount));
        OnPropertyChanged(nameof(AudioFileCount));
        OnPropertyChanged(nameof(VideoFileCount));
        OnPropertyChanged(nameof(TotalMediaFiles));
        OnPropertyChanged(nameof(TotalSizeBytes));
        OnPropertyChanged(nameof(TotalSize));
    }
}