using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.Core.Models.Discovery;

namespace DJLibraryManager.UI.Models.Discovery;

public partial class MediaLocationDiscoveryInfo : ObservableObject
{
    /// <summary>
    /// Discovery summary for this media location.
    /// </summary>
    public required MediaLocationDiscoverySummary Summary { get; init; }

    public string Path => Summary.MediaLocation.Path;

    public int FolderCount => Summary.FolderCount;

    public int AudioFileCount => Summary.AudioFileCount;

    public int VideoFileCount => Summary.VideoFileCount;

    public int TotalMediaFiles => Summary.TotalMediaFiles;

    /// <summary>
    /// True when the current filesystem differs from the last discovery.
    /// </summary>
    [ObservableProperty]
    private bool hasChanges;

    /// <summary>
    /// Current discovery status.
    /// </summary>
    public string Status =>
        !Summary.MediaLocation.Exists
            ? "Location Not Available"
        : Summary.DiscoveryDate is null
            ? "Ready to Discover"
        : HasChanges
            ? "Changes Detected"
            : "Discovery Complete";

    /// <summary>
    /// Status colour.
    /// </summary>
    public IBrush StatusBrush =>
        Status switch
        {
            "Discovery Complete" => Brushes.LimeGreen,
            "Changes Detected" => Brushes.Goldenrod,
            "Location Not Available" => Brushes.DarkOrange,
            _ => Brushes.Gray
        };

    /// <summary>
    /// View is only available when discovery has completed
    /// and no changes have been detected.
    /// </summary>
    public bool CanView =>
        Summary.DiscoveryDate is not null &&
        !HasChanges;

    /// <summary>
    /// Discover is available when:
    /// - the location has never been discovered, or
    /// - changes have been detected.
    /// </summary>
    public bool CanDiscover =>
        Summary.MediaLocation.Exists &&
        (
            Summary.DiscoveryDate is null ||
            HasChanges
        );

    partial void OnHasChangesChanged(bool value)
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusBrush));
        OnPropertyChanged(nameof(CanView));
        OnPropertyChanged(nameof(CanDiscover));
    }
}