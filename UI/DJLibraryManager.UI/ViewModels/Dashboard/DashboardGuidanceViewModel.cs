using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.Core.Workflow;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DJLibraryManager.UI.ViewModels.Dashboard;

/// <summary>
/// Represents the contextual guidance displayed on the Dashboard.
/// </summary>
public partial class DashboardGuidanceViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string nextStep = string.Empty;

    [ObservableProperty]
    private bool isWelcome;

    public ObservableCollection<string> WhatHappens { get; } = new();

    public ObservableCollection<string> GoodToKnow { get; } = new();

    public DashboardGuidanceViewModel()
    {
        Show(WorkflowStage.Welcome);
    }

    public void Show(WorkflowStage stage)
    {
        IsWelcome = false;

        switch (stage)
        {
            case WorkflowStage.Discovery:
                Update(
                    "Discovery Stage",
                    string.Empty,
                    "Review the discovered providers and media locations.",
                    new[]
                    {
                        "Detect installed DJ software",
                        "Locate music folders",
                        "Prepare for Import"
                    },
                    new[]
                    {
                        "Safe to run at any time",
                        "No files are modified",
                        "Supports multiple DJ applications"
                    });
                break;

            case WorkflowStage.Import:
                Update(
                    "Import Stage",
                    string.Empty,
                    "Import each discovered provider.",
                    new[]
                    {
                        "Read playlists",
                        "Import track metadata",
                        "Build the DIASISS library"
                    },
                    new[]
                    {
                        "Original libraries remain unchanged",
                        "Import providers individually",
                        "Repeat imports whenever needed"
                    });
                break;

            case WorkflowStage.Analysis:
                Update(
                    "Analysis Stage",
                    string.Empty,
                    "Run a full library analysis.",
                    new[]
                    {
                        "Calculate library health",
                        "Detect missing files",
                        "Find duplicate tracks"
                    },
                    new[]
                    {
                        "Analysis is read-only",
                        "Large libraries take longer",
                        "Results drive later stages"
                    });
                break;

            case WorkflowStage.Search:
                Update(
                    "Search Stage",
                    string.Empty,
                    "Search your imported music library.",
                    new[]
                    {
                        "Search every provider",
                        "Filter large collections",
                        "Open results instantly"
                    },
                    new[]
                    {
                        "Search is instant",
                        "Use multiple filters",
                        "Results update live"
                    });
                break;

            case WorkflowStage.Improve:
                Update(
                    "Improve Stage",
                    string.Empty,
                    "Review suggested improvements.",
                    new[]
                    {
                        "Clean metadata",
                        "Standardise values",
                        "Prepare changes"
                    },
                    new[]
                    {
                        "Nothing changes automatically",
                        "Review every suggestion",
                        "Approve only what you want"
                    });
                break;

            case WorkflowStage.Structure:
                Update(
                    "Structure Stage",
                    string.Empty,
                    "Approve or reject suggested changes.",
                    new[]
                    {
                        "Review recommendations",
                        "Preview every change",
                        "Queue approved updates"
                    },
                    new[]
                    {
                        "Changes aren't written yet",
                        "Everything can be reviewed",
                        "Submit is the final step"
                    });
                break;

            case WorkflowStage.Synchronise:
                Update(
                    "Synchronise Stage",
                    string.Empty,
                    "Review pending changes before submitting.",
                    new[]
                    {
                        "Write approved changes",
                        "Update DJ libraries",
                        "Complete the workflow"
                    },
                    new[]
                    {
                        "Backup before writing",
                        "Provider-safe updates",
                        "Changes are permanent"
                    });
                break;

            default:
                Reset();
                break;
        }
    }

    public void Update(
        string title,
        string description,
        string nextStep,
        IEnumerable<string> whatHappens,
        IEnumerable<string> goodToKnow)
    {
        Title = title;
        Description = description;
        NextStep = nextStep;

        WhatHappens.Clear();

        foreach (var item in whatHappens)
        {
            WhatHappens.Add(item);
        }

        GoodToKnow.Clear();

        foreach (var item in goodToKnow)
        {
            GoodToKnow.Add(item);
        }
    }

    public void Reset()
    {
        IsWelcome = true;

        Update(
            "Welcome to DIASISS DJ",
            string.Empty,
            "Hover over a workflow stage to begin.",
            new[]
            {
                "Discover installed DJ software",
                "Import your music library",
                "Analyse library health"
            },
            new[]
            {
                "Hover over a workflow stage",
                "Complete stages from left to right",
                "Progress is saved automatically"
            });
    }
}