using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.Core.Workflow;

/// <summary>
/// Central definition of the DIASISS workflow.
/// This is the single source of truth for all workflow stages.
/// </summary>
public static class WorkflowDefinitions
{
    public static readonly WorkflowDefinition Discovery = new()
    {
        Stage = WorkflowStage.Discovery,
        Order = 1,
        Name = "Discovery",
        Icon = "🔍",
        Description = "Discover installed DJ software and music locations."
    };

    public static readonly WorkflowDefinition Import = new()
    {
        Stage = WorkflowStage.Import,
        Order = 2,
        Name = "Import",
        Icon = "📥",
        Description = "Import provider libraries and local music into the DIASISS library."
    };

    public static readonly WorkflowDefinition Analysis = new()
    {
        Stage = WorkflowStage.Analysis,
        Order = 3,
        Name = "Analysis",
        Icon = "📊",
        Description = "Analyse the health and quality of your complete music library."
    };

    public static readonly WorkflowDefinition Search = new()
    {
        Stage = WorkflowStage.Search,
        Order = 4,
        Name = "Search",
        Icon = "🔎",
        Description = "Search for missing metadata, duplicates and inconsistencies."
    };

    public static readonly WorkflowDefinition Improve = new()
    {
        Stage = WorkflowStage.Improve,
        Order = 5,
        Name = "Improve",
        Icon = "✨",
        Description = "Improve and enrich the metadata throughout your library."
    };

    public static readonly WorkflowDefinition Structure = new()
    {
        Stage = WorkflowStage.Structure,
        Order = 6,
        Name = "Structure",
        Icon = "📂",
        Description = "Design the optimal folder structure for your music collection."
    };

    public static readonly WorkflowDefinition Synchronise = new()
    {
        Stage = WorkflowStage.Synchronise,
        Order = 7,
        Name = "Synchronise",
        Icon = "💾",
        Description = "Apply approved changes and synchronise them back to your DJ software."
    };

    /// <summary>
    /// Returns every workflow stage in the correct order.
    /// </summary>
    public static IReadOnlyList<WorkflowDefinition> All { get; } =
        new[]
        {
            Discovery,
            Import,
            Analysis,
            Search,
            Improve,
            Structure,
            Synchronise
        }
        .OrderBy(x => x.Order)
        .ToList();

    /// <summary>
    /// Returns the definition for a workflow stage.
    /// </summary>
    public static WorkflowDefinition Get(WorkflowStage stage)
    {
        return All.Single(x => x.Stage == stage);
    }
}