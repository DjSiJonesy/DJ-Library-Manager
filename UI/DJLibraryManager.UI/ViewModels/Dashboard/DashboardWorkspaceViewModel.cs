using Avalonia.Media;
using DJLibraryManager.UI.Models;
using System.Collections.ObjectModel;
using DJLibraryManager.Core.Workflow;
using System.Linq;

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

    public void UpdateDiscoveryStatus()
    {
        if (WorkflowCards.Count == 0)
            return;

        var discoverCard = WorkflowCards[0];

        var providerCount = _dashboard.InstalledProviders.Count(x => x.Installed);
        var mediaLocationCount = _dashboard.MediaLocations.Count;

        discoverCard.PrimaryStatisticTitle = "Providers Found";
        discoverCard.PrimaryStatisticValue = providerCount.ToString();

        discoverCard.SecondaryStatisticTitle = "Media Locations";
        discoverCard.SecondaryStatisticValue = mediaLocationCount.ToString();

        if (providerCount == 0)
        {
            discoverCard.Status = "Waiting";
            discoverCard.StatusBrush = Brushes.DeepSkyBlue;
        }
        else if (mediaLocationCount == 0)
        {
            discoverCard.Status = "Partial";
            discoverCard.StatusBrush = Brushes.Goldenrod;
        }
        else
        {
            discoverCard.Status = "Complete";
            discoverCard.StatusBrush = Brushes.LimeGreen;
        }
    }

    public void UpdateImportStatus()
    {
        if (WorkflowCards.Count < 2)
            return;

        var importCard = WorkflowCards[1];

        var installedProviders =
            _dashboard.InstalledProviders.Count(p => p.Installed);

        var importedProviders =
            _dashboard.InstalledProviders.Count(p =>
                p.Installed &&
                p.ImportState == ImportState.Imported);

        var tracks =
            _dashboard.InstalledProviders.Sum(p => p.TrackCount);

        var playlists =
            _dashboard.InstalledProviders.Sum(p => p.PlaylistCount);

        importCard.PrimaryStatisticTitle = "Tracks Imported";
        importCard.PrimaryStatisticValue = tracks.ToString("N0");

        importCard.SecondaryStatisticTitle = "Playlists";
        importCard.SecondaryStatisticValue = playlists.ToString("N0");

        if (importedProviders == 0)
        {
            importCard.Status = "Waiting";
            importCard.StatusBrush = Brushes.DeepSkyBlue;
        }
        else if (importedProviders < installedProviders)
        {
            importCard.Status = "Partial";
            importCard.StatusBrush = Brushes.Goldenrod;
        }
        else
        {
            importCard.Status = "Complete";
            importCard.StatusBrush = Brushes.LimeGreen;
        }
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

            Status = GetDiscoveryStatus(),
            StatusBrush = GetDiscoveryStatusBrush(),

            PrimaryStatisticTitle = "Providers Found",
            PrimaryStatisticValue = _dashboard.InstalledProviders
                .Count(x => x.Installed)
                .ToString(),

            SecondaryStatisticTitle = "Media Locations",
            SecondaryStatisticValue = _dashboard.MediaLocations
                .Count
                .ToString()
        });

        WorkflowCards.Add(new WorkflowCardViewModel
        {
            Definition = WorkflowDefinitions.Import,
            HoverAction = stage => Guidance.Show(stage),

            Status = GetImportStatus(),
            StatusBrush = GetImportStatusBrush(),

            PrimaryStatisticTitle = "Tracks Imported",
            PrimaryStatisticValue = _dashboard.InstalledProviders
                .Sum(x => x.TrackCount)
                .ToString("N0"),

            SecondaryStatisticTitle = "Playlists",
            SecondaryStatisticValue = _dashboard.InstalledProviders
                .Sum(x => x.PlaylistCount)
                .ToString("N0")
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

        Guidance.Reset();
    }

    private static (string Status, IBrush Brush) GetWorkflowStatus(
     int completed,
     int total)
    {
        if (completed == 0)
        {
            return ("Waiting", Brushes.DeepSkyBlue);
        }

        if (completed < total)
        {
            return ("Partial", Brushes.Goldenrod);
        }

        return ("Complete", Brushes.LimeGreen);
    }

    private string GetDiscoveryStatus()
    {
        var installedProviders =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var mediaLocations =
            _dashboard.MediaLocations.Count;

        return GetWorkflowStatus(
            mediaLocations,
            installedProviders).Status;
    }

    private IBrush GetDiscoveryStatusBrush()
    {
        var installedProviders =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var mediaLocations =
            _dashboard.MediaLocations.Count;

        return GetWorkflowStatus(
            mediaLocations,
            installedProviders).Brush;
    }

    private string GetImportStatus()
    {
        var installedProviders =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var importedProviders =
            _dashboard.InstalledProviders.Count(x =>
                x.Installed &&
                x.ImportState == ImportState.Imported);

        return GetWorkflowStatus(
            importedProviders,
            installedProviders).Status;
    }

    private IBrush GetImportStatusBrush()
    {
        var installedProviders =
            _dashboard.InstalledProviders.Count(x => x.Installed);

        var importedProviders =
            _dashboard.InstalledProviders.Count(x =>
                x.Installed &&
                x.ImportState == ImportState.Imported);

        return GetWorkflowStatus(
            importedProviders,
            installedProviders).Brush;
    }
}