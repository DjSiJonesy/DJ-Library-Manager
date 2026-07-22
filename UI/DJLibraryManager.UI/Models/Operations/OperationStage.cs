namespace DJLibraryManager.UI.Models.Operations;

/// <summary>
/// Represents the overall lifecycle state of a long-running operation.
/// </summary>
public enum OperationStage
{
    /// <summary>
    /// No operation is active.
    /// </summary>
    None,

    /// <summary>
    /// The operation has been created but not yet started.
    /// </summary>
    Pending,

    /// <summary>
    /// The operation is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The operation was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The operation failed.
    /// </summary>
    Failed
}