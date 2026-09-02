namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents one physical file change recorded in the DIASISS
/// FileChanges table.
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

    public string MediaId { get; set; } =
        string.Empty;

    public string OriginalPath { get; set; } =
        string.Empty;

    public string NewPath { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;

    public string ChangedDate { get; set; } =
        string.Empty;
}