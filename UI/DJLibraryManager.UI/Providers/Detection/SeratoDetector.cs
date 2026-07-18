using System;
using System.IO;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Serato DJ Pro installation.
/// </summary>
public class SeratoDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        var result = FindInstalledApplication.Find(
            providerName: "Serato",
            executables:
            [
                "Serato DJ Pro.exe",
                "Serato DJ Lite.exe"
            ],
            installPaths:
            [
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Serato"),

                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Serato")
            ]);

        // If Serato isn't installed there's nothing else to discover.
        if (!result.Installed)
        {
            return result;
        }

        var settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "_Serato_");

        result.SettingsPath = settingsFolder;

        var database = Path.Combine(settingsFolder, "database V2");

        if (File.Exists(database))
        {
            result.DatabasePath = database;
        }

        return result;
    }
}