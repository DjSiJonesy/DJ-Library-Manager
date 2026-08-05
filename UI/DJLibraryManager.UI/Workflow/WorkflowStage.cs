namespace DJLibraryManager.Core.Workflow;

/// <summary>
/// Represents the seven stages of the DIASISS workflow.
/// These stages define the primary application workflow and
/// are used consistently throughout the application.
/// </summary>
public enum WorkflowStage
{
    /// <summary>
    /// Welcome / Landing page.
    /// </summary>
    Welcome = 0,

    /// <summary>
    /// Discover installed DJ applications and music locations.
    /// </summary>
    Discovery = 1,

    /// <summary>
    /// Import provider libraries and local music into DIASISS.
    /// </summary>
    Import = 2,

    /// <summary>
    /// Analyse the health and quality of the library.
    /// </summary>
    Analysis = 3,

    /// <summary>
    /// Search for missing metadata and library issues.
    /// </summary>
    Search = 4,

    /// <summary>
    /// Improve and enrich library metadata.
    /// </summary>
    Improve = 5,

    /// <summary>
    /// Design the optimal folder structure.
    /// </summary>
    Structure = 6,

    /// <summary>
    /// Synchronise approved changes back to the DJ software.
    /// </summary>
    Synchronise = 7
}