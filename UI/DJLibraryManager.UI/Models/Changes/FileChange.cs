using System;

namespace DJLibraryManager.UI.Models.Changes;

/// <summary>
/// Represents a physical file change made by a DIASISS
/// workflow stage.
///
/// FileChanges are provider-independent and are used by
/// both Improve and Structure to provide an auditable
/// record of physical file movements and safe rollback.
///
/// The DIASISS MediaId is the authoritative identity of
/// the
/// media item involved in the change.
/// </summary>
public sealed class FileChange
{
    // ============================================================
    // Identity
    // ============================================================

    /// <summary>
    /// Gets the unique database identifier for this change.
    /// </summary>
    public long ChangeId { get; set; }

    /// <summary>
    /// Gets the workflow operation that created this change.
    ///
    /// All file changes belonging to one Confirm & Apply or
    /// Structure operation share the same OperationId.
    /// </summary>
    public string OperationId { get; set; } =
        string.Empty;

    // ============================================================
    // Workflow
    // ============================================================

    /// <summary>
    /// Gets the workflow stage which created the change.
    ///
    /// Expected values include:
    ///
    /// Improve
    /// Structure
    /// </summary>
    public string Stage { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets the specific type of change.
    ///
    /// Examples:
    ///
    /// Duplicate
    /// Structure
    /// </summary>
    public string ChangeType { get; set; } =
        string.Empty;

    // ============================================================
    // Media Identity
    // ============================================================

    /// <summary>
    /// Gets the DIASISS MediaId associated with the physical file.
    ///
    /// This is the authoritative DIASISS GUID used to verify
    /// that a rollback is operating on the expected media item.
    /// </summary>
    public string MediaId { get; set; } =
        string.Empty;

    // ============================================================
    // File Locations
    // ============================================================

    /// <summary>
    /// Gets the physical file location before the change.
    /// </summary>
    public string OriginalPath { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets the physical file location after the change.
    /// </summary>
    public string NewPath { get; set; } =
        string.Empty;

    // ============================================================
    // Change State
    // ============================================================

    /// <summary>
    /// Gets the current state of the physical file change.
    ///
    /// Expected values include:
    ///
    /// Pending
    /// Completed
    /// Failed
    /// RolledBack
    /// </summary>
    public string Status { get; set; } =
        string.Empty;

    // ============================================================
    // Dates
    // ============================================================

    /// <summary>
    /// Gets the UTC date/time when the change was recorded.
    /// </summary>
    public DateTime ChangedDate { get; set; }
}