using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;
using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// Workspace displayed when a media location is selected.
/// </summary>
public partial class MediaLocationWorkspaceViewModel : WorkspaceViewModel
{
    private readonly MediaLibraryDiscoveryService _discoveryService = new();
    private readonly DiscoveryRepository _discoveryRepository = App.Services.DiscoveryRepository;

    public MediaLocation MediaLocation { get; }
    public event EventHandler? GoBackRequested;

    /// <summary>
    /// Libraries discovered beneath this media location.
    /// </summary>
    public ObservableCollection<MediaLibrary> Libraries { get; } = new();

    public override string Title => "Media Location";

    public MediaLocationWorkspaceViewModel(MediaLocation mediaLocation)
    {
        MediaLocation = mediaLocation;

        if (!Exists)
        {
            Status = "Location Not Available";
            return;
        }

        // Restore any previous discovery for this media location.
        var session = _discoveryRepository.Get(Path);

        if (session is not null)
        {
            foreach (var library in session.Libraries)
            {
                Libraries.Add(library);
            }

            Status = "Discovery Complete";
            RefreshDiscoverySummary();
        }
        else
        {
            Status = "Ready to Discover";
        }
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

    /// <summary>
    /// Returns to the Dashboard.
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackRequested?.Invoke(this, EventArgs.Empty);
    }

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

    #region Private Helpers

    /// <summary>
    /// Refreshes the calculated discovery summary properties.
    /// </summary>
    private void RefreshDiscoverySummary()
    {
        OnPropertyChanged(nameof(FolderCount));
        OnPropertyChanged(nameof(AudioFileCount));
        OnPropertyChanged(nameof(VideoFileCount));
        OnPropertyChanged(nameof(TotalMediaFiles));
        OnPropertyChanged(nameof(TotalSizeBytes));
        OnPropertyChanged(nameof(TotalSize));
    }

    #endregion

    #region Commands

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

        var session = new DiscoverySession
        {
            MediaLocation = MediaLocation,
            Libraries = libraries,
            DiscoveryDate = DateTime.Now
        };

        _discoveryRepository.Save(session);

        foreach (var library in session.Libraries)
        {
            Libraries.Add(library);
        }

        Status = "Discovery Complete";

        RefreshDiscoverySummary();
    }

    #endregion
}