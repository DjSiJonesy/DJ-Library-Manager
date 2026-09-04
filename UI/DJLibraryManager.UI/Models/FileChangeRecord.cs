namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents one change recorded in the DIASISS FileChanges table.
///
/// FileChanges can record either:
///
///     Physical file changes
///     Provider database changes
///
/// Physical file changes use OriginalPath and NewPath.
///
/// Provider database changes additionally retain the provider identity,
/// provider database path and the original provider record data required
/// for future recovery.
/// </summary>
public sealed class FileChangeRecord
{
    public long ChangeId { get; set; }

    public string OperationId { get; set; } =
        string.Empty;

    public string Stage { get; set; } =
        string.Empty;

    public string ChangeType { get; set; } =
        string.Empty;

    /// <summary>
    /// The authoritative DIASISS MediaId associated with the change.
    /// </summary>
    public string MediaId { get; set; } =
        string.Empty;

    /// <summary>
    /// Original physical path associated with the change.
    ///
    /// For physical file operations this is the source path.
    ///
    /// For provider database changes this identifies the media record's
    /// physical file path.
    /// </summary>
    public string OriginalPath { get; set; } =
        string.Empty;

    /// <summary>
    /// New physical path associated with a physical file operation.
    ///
    /// Provider database changes do not use this field.
    /// </summary>
    public string NewPath { get; set; } =
        string.Empty;

    /// <summary>
    /// SQLite ProviderId associated with a provider database change.
    /// </summary>
    public long? ProviderId { get; set; }

    /// <summary>
    /// Full path to the provider database that was changed.
    /// </summary>
    public string? ProviderDatabasePath { get; set; }

    /// <summary>
    /// Original provider database record data captured before the
    /// provider record was removed or modified.
    ///
    /// This is retained so the provider change can be recovered later.
    /// </summary>
    public string? ProviderRecordData { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public string ChangedDate { get; set; } =
        string.Empty;
}