namespace DJLibraryManager.Core.Workflow;

/// <summary>
/// Defines a single stage within the DIASISS workflow.
/// </summary>
public sealed class WorkflowDefinition
{
    /// <summary>
    /// The workflow stage.
    /// </summary>
    public required WorkflowStage Stage { get; init; }

    /// <summary>
    /// The order in which the stage appears within the workflow.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// The display name shown throughout the application.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A short description of the stage.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The icon displayed for the stage.
    /// </summary>
    public required string Icon { get; init; }

    /// <summary>
    /// Indicates whether this stage is currently available.
    /// This allows unfinished workflow stages to be disabled
    /// without changing the workflow definition.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}