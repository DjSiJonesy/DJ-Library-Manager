using System.Collections.Generic;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.UI.Models.LibraryExplorer;

/// <summary>
/// Represents everything currently known by DIASISS DJ.
/// This model is consumed by the Library Explorer workspace.
/// </summary>
public sealed class LibraryExplorerModel
{
    /// <summary>
    /// Overall discovery summary.
    /// </summary>
    public required LibraryExplorerSummary Summary { get; init; }

    /// <summary>
    /// All known media locations together with everything
    /// DIASISS DJ currently knows about them.
    /// </summary>
    public required IReadOnlyList<MediaLocationExplorerItem> MediaLocations { get; init; }
}