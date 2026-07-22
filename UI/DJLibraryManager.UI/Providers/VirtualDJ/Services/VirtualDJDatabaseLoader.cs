using System;
using System.IO;
using System.Xml.Linq;
using DJLibraryManager.UI.Providers.VirtualDJ.Models;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Services;

/// <summary>
/// Loads a VirtualDJ database.xml file.
/// </summary>
public sealed class VirtualDJDatabaseLoader
{
    /// <summary>
    /// Loads a VirtualDJ database from disk.
    /// </summary>
    /// <param name="path">
    /// Full path to the VirtualDJ database.xml file.
    /// </param>
    /// <returns>
    /// A loaded <see cref="VirtualDJDatabase"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Path is null or empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Database file not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Database could not be loaded.
    /// </exception>
    public VirtualDJDatabase Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "A database path must be supplied.",
                nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "VirtualDJ database file not found.",
                path);
        }

        try
        {
            var xml = XDocument.Load(path, LoadOptions.PreserveWhitespace);

            return new VirtualDJDatabase
            {
                Path = Path.GetFullPath(path),
                Xml = xml,
                Loaded = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Unable to load the VirtualDJ database.",
                ex);
        }
    }
}