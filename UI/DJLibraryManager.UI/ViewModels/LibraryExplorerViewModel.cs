using System.Linq;

using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models.LibraryExplorer;

namespace DJLibraryManager.UI.ViewModels;

/// <summary>
/// ViewModel for the Library Explorer workspace.
/// Displays a consolidated view of everything currently known
/// by DIASISS DJ.
/// </summary>
public sealed class LibraryExplorerViewModel : WorkspaceViewModel
{
    private readonly ExplorerService _explorerService;

    public override string Title => "Library Explorer";

    /// <summary>
    /// Everything currently known by the application.
    /// </summary>
    public LibraryExplorerModel Explorer { get; }

    public LibraryExplorerViewModel()
    {
        _explorerService = new ExplorerService(
            App.Services.MediaLocationRepository,
            App.Services.DiscoveryRepository);

        var explorerItems = _explorerService.BuildExplorer();

        Explorer = new LibraryExplorerModel
        {
            Summary = new LibraryExplorerSummary
            {
                MediaLocationCount = explorerItems.Count,

                LibraryCount = explorerItems.Sum(x => x.FolderCount),

                AudioFileCount = explorerItems.Sum(x => x.AudioFileCount),

                VideoFileCount = explorerItems.Sum(x => x.VideoFileCount),

                TotalSizeBytes = explorerItems.Sum(x =>
                    x.DiscoverySession?.TotalSizeBytes ?? 0)
            },

            MediaLocations = explorerItems
        };
    }
}