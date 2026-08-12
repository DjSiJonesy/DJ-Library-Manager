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

    // ============================================================
    // Data Folders
    // ============================================================

    public static string Data =>
        Path.Combine(Root, "Data");

    public static string Analysis =>
        Path.Combine(Data, "Analysis");

    public static string Discovery =>
        Path.Combine(Data, "Discovery");

    public static string Import =>
        Path.Combine(Data, "Import");

    public static string Library =>
        Path.Combine(Data, "Library");

    public static string Search =>
        Path.Combine(Data, "Search");

    // ============================================================
    // Data Files
    // ============================================================

    public static string DiscoverySessions =>
        Path.Combine(Discovery, "DiscoverySessions.json");

    public static string DiscoveryValidation =>
        Path.Combine(Discovery, "DiscoveryValidation.json");

    public static string MediaImports =>
        Path.Combine(Import, "MediaImports.json");

    public static string LibraryMetadata =>
        Path.Combine(Import, "LibraryMetadata.json");

    public static string MediaLibrary =>
        Path.Combine(Library, "MediaLibrary.json");

    public static string LatestAnalysis =>
        Path.Combine(Analysis, "LatestAnalysis.json");

    public static string LatestSearch =>
        Path.Combine(Search, "LatestSearch.json");

    // ============================================================
    // Other Folders
    // ============================================================

    public static string Logs =>
        Path.Combine(Root, "Logs");

    public static string Backups =>
        Path.Combine(Root, "Backups");

    public static string Temp =>
        Path.Combine(Root, "Temp");

    public static string Cache =>
        Path.Combine(Root, "Cache");

    /// <summary>
    /// Creates the application folder structure.
    /// Safe to call multiple times.
    /// </summary>
    public static void EnsureCreated()
    {
        Directory.CreateDirectory(Root);

        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Analysis);
        Directory.CreateDirectory(Discovery);
        Directory.CreateDirectory(Import);
        Directory.CreateDirectory(Library);
        Directory.CreateDirectory(Search);

        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Backups);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Cache);
    }
}