using System.Collections.ObjectModel;

namespace DJLibraryManager.UI.Models.Explorer;

/// <summary>
/// Represents a node displayed within the DIASISS DJ Explorer.
/// </summary>
public sealed class ExplorerNode
{
    /// <summary>
    /// Display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Full filesystem path.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Child nodes.
    /// </summary>
    public ObservableCollection<ExplorerNode> Children { get; } = new();

    /// <summary>
    /// Indicates whether the node is expanded.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Indicates whether the node is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// True if this node has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;
}