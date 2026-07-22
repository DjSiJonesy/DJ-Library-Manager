using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Models;

/// <summary>
/// Represents a loaded VirtualDJ database.
/// </summary>
public sealed class VirtualDJDatabase
{
    /// <summary>
    /// Full path to the database.xml file.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// The loaded XML document.
    /// </summary>
    public required XDocument Xml { get; init; }

    /// <summary>
    /// Date/time the database was loaded.
    /// </summary>
    public DateTime Loaded { get; init; } = DateTime.Now;

    /// <summary>
    /// All Song elements contained in the database.
    /// </summary>
    public IEnumerable<XElement> Songs =>
        Xml.Root?
           .Elements("Song")
        ?? Enumerable.Empty<XElement>();
}