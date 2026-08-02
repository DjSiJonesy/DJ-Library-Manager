using System;
using System.IO;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Centralises all application storage locations.
/// </summary>
public static class ApplicationPaths
{
    /// <summary>
    /// Root application data folder.
    /// </summary>
    public static string Root =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DIASISS DJ");

    /// <summary>
    /// Discovery repository.
    /// </summary>
    public static string DiscoverySessions =>
        Path.Combine(Root, "DiscoverySessions.json");

    /// <summary>
    /// Imported media library.
    /// </summary>
    public static string MediaLibrary =>
        Path.Combine(Root, "MediaLibrary.json");

    /// <summary>
    /// Provider import metadata.
    /// </summary>
    public static string LibraryMetadata =>
        Path.Combine(Root, "LibraryMetadata.json");

    /// <summary>
    /// Application log folder.
    /// </summary>
    public static string Logs =>
        Path.Combine(Root, "Logs");

    /// <summary>
    /// Backup folder.
    /// </summary>
    public static string Backups =>
        Path.Combine(Root, "Backups");

    /// <summary>
    /// Temporary working folder.
    /// </summary>
    public static string Temp =>
        Path.Combine(Root, "Temp");

    /// <summary>
    /// Cached external metadata.
    /// </summary>
    public static string Cache =>
        Path.Combine(Root, "Cache");

    /// <summary>
    /// Creates the application folder structure.
    /// Safe to call multiple times.
    /// </summary>
    public static void EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Backups);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Cache);
    }
}