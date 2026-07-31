using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.Input;

using DJLibraryManager.Core.Models;
using DJLibraryManager.UI.Services;

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
    }

    /// <summary>
    /// Friendly name of the media location.
    /// </summary>
    public string Name => MediaLocation.Name;

    /// <summary>
    /// Root folder.
    /// </summary>
    public string Path => MediaLocation.Path;

    /// <summary>
    /// Drive containing the media location.
    /// </summary>
    public string Drive => MediaLocation.Drive;

    /// <summary>
    /// Indicates whether the location currently exists.
    /// </summary>
    public bool Exists => MediaLocation.Exists;

    /// <summary>
    /// Friendly status text.
    /// </summary>
    public string Status =>
        Exists
            ? "Ready to Discover"
            : "Location Not Available";

    /// <summary>
    /// Discovers media libraries beneath the selected media location.
    /// </summary>
    [RelayCommand]
    private void DiscoverMedia()
    {
        Libraries.Clear();

        if (!Exists)
            return;

        var libraries = _discoveryService.DiscoverLibraries(MediaLocation);

        foreach (var library in libraries)
        {
            Libraries.Add(library);
        }
    }
}