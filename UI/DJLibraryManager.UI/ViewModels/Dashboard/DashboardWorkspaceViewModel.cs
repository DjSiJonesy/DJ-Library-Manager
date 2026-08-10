using Avalonia.Media;
using DJLibraryManager.Core.Services;
using DJLibraryManager.Core.Workflow;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Services.Discovery;
using DJLibraryManager.UI.Services.Import;
using DJLibraryManager.UI.ViewModels.Workspace;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.ViewModels.Dashboard;

public class DashboardWorkspaceViewModel : WorkspaceViewModel
{
    public override string Title => "Dashboard";

    public ObservableCollection<WorkflowCardViewModel> WorkflowCards { get; } = new();

    /// <summary>
    /// Contextual guidance displayed on the Dashboard.
    /// </summary>
    public DashboardGuidanceViewModel Guidance { get; } = new();

    private readonly DashboardViewModel _dashboard;
    private readonly DiscoveryValidationService _validationService = new();
    private readonly ImportValidationService _importValidationService = new();

    public void UpdateDiscoveryStatus()
    {
        if (WorkflowCards.Count == 0)
            return;

        var discoverCard = WorkflowCards[0];

        var providerCount =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var mediaLocationCount =
            _dashboard.MediaLocations.Count;

        discoverCard.PrimaryStatisticTitle = "Providers Found";
        discoverCard.PrimaryStatisticValue = providerCount.ToString();

        discoverCard.SecondaryStatisticTitle = "Media Locations";
        discoverCard.SecondaryStatisticValue = mediaLocationCount.ToString();

        var discoverySessions =
            App.Services.DiscoveryRepository.DiscoverySessions;

        //
        // Not all media locations have been discovered.
        //

        if (discoverySessions.Count < mediaLocationCount)
        {
            discoverCard.Status = "Ready to Discover";
            discoverCard.StatusBrush = Brushes.DeepSkyBlue;
            return;
        }

        //
        // One or more discovered locations have changed.
        //

        //
        // Validation will be performed explicitly by the
        // Discovery workspace (Recheck Drives).
        //

        //
        // Everything is up to date.
        //

        discoverCard.Status = "Discovery Complete";
        discoverCard.StatusBrush = Brushes.LimeGreen;
    }

    public async Task UpdateImportStatus()
    {
        if (WorkflowCards.Count < 2)
            return;

        var importCard = WorkflowCards[1];

        var installedProviders =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var importedProviders =
            _dashboard.InstalledProviders.Count(x =>
                x.Installed &&
                x.ImportState == ImportState.Imported);

        var statistics =
            await App.Services
                .LibraryStatisticsService
                .GetStatisticsAsync();

                importCard.PrimaryStatisticTitle = "Tracks Imported";
                importCard.PrimaryStatisticValue =
                    statistics.LibraryTrackCount.ToString("N0");

                importCard.SecondaryStatisticTitle = "Playlists";
                importCard.SecondaryStatisticValue =
                    statistics.LibraryPlaylistCount.ToString("N0");

        //
        // Not all providers imported.
        //

        if (importedProviders == 0)
        {
            importCard.Status = "Ready to Import";
            importCard.StatusBrush = Brushes.DeepSkyBlue;
            return;
        }

        //
        // Not all discovered media locations imported.
        //

        var discoverySessions =
            App.Services.DiscoveryRepository.DiscoverySessions;

        //
        // Import cannot be complete until Discovery is complete.
        //

        if (discoverySessions.Count < _dashboard.MediaLocations.Count)
        {
            importCard.Status = "Ready to Import";
            importCard.StatusBrush = Brushes.DeepSkyBlue;
            return;
        }

        var importRepository =
            App.Services.MediaImportRepository;

        foreach (var session in discoverySessions)
        {
            var importRecord =
                importRepository.Get(session.MediaLocation.Path);

            if (importRecord is null)
            {
                importCard.Status = "Ready to Import";
                importCard.StatusBrush = Brushes.DeepSkyBlue;
                return;
            }

            if (_importValidationService.HasChanges(
                session,
                importRecord))
            {
                importCard.Status = "Changes Detected";
                importCard.StatusBrush = Brushes.Goldenrod;
                return;
            }
        }

        //
        // Everything is fully imported and current.
        //

        importCard.Status = "Import Complete";
        importCard.StatusBrush = Brushes.LimeGreen;
    }

    public DashboardWorkspaceViewModel(
        DashboardViewModel dashboard)
    {
        _dashboard = dashboard;

        var discovery = WorkflowDefinitions.Discovery;
        var import = WorkflowDefinitions.Import;
        var analysis = WorkflowDefinitions.Analysis;
        var search = WorkflowDefinitions.Search;
        var improve = WorkflowDefinitions.Improve;
        var structure = WorkflowDefinitions.Structure;
        var synchronise = WorkflowDefinitions.Synchronise;

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Discovery,
            HoverAction = stage => Guidance.Show(stage),
            ActionCommand = _dashboard.OpenDiscoveryCommand,

            PrimaryStatisticTitle = "Providers Found",
            PrimaryStatisticValue = string.Empty,

            SecondaryStatisticTitle = "Media Locations",
            SecondaryStatisticValue = string.Empty
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Import,
            HoverAction = stage => Guidance.Show(stage),
            ActionCommand = _dashboard.OpenImportCommand,

            Status = "Ready to Import",
            StatusBrush = Brushes.DeepSkyBlue,

            PrimaryStatisticTitle = "Tracks Imported",
            PrimaryStatisticValue = "0",

            SecondaryStatisticTitle = "Playlists",
            SecondaryStatisticValue = "0"
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Analysis,
            HoverAction = stage => Guidance.Show(stage),

            Status = "Ready",
            StatusBrush = Brushes.DeepSkyBlue,

            PrimaryStatisticTitle = "Health Score",
            PrimaryStatisticValue = "84%",

            SecondaryStatisticTitle = "Issues Found",
            SecondaryStatisticValue = "126"
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Search,
            HoverAction = stage => Guidance.Show(stage),

            Status = "Available",
            StatusBrush = Brushes.DeepSkyBlue,

            PrimaryStatisticTitle = "Indexed Tracks",
            PrimaryStatisticValue = "12,246",

            SecondaryStatisticTitle = "Playlists",
            SecondaryStatisticValue = "487"
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Improve,
            HoverAction = stage => Guidance.Show(stage),

            Status = "Waiting",
            StatusBrush = Brushes.Goldenrod,

            PrimaryStatisticTitle = "Suggestions",
            PrimaryStatisticValue = "412",

            SecondaryStatisticTitle = "Auto Fixes",
            SecondaryStatisticValue = "97"
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Structure,
            HoverAction = stage => Guidance.Show(stage),

            Status = "Waiting",
            StatusBrush = Brushes.Goldenrod,

            PrimaryStatisticTitle = "Recommendations",
            PrimaryStatisticValue = "126",

            SecondaryStatisticTitle = "Approved",
            SecondaryStatisticValue = "0"
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Synchronise,
            HoverAction = stage => Guidance.Show(stage),

            Status = "Waiting",
            StatusBrush = Brushes.Goldenrod,

            PrimaryStatisticTitle = "Pending Changes",
            PrimaryStatisticValue = "0",

            SecondaryStatisticTitle = "Ready",
            SecondaryStatisticValue = "No"
        });

        UpdateDiscoveryStatus();
        _ = UpdateImportStatus();

        Guidance.Reset();
    } 
}