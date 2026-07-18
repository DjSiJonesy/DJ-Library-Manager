using System;
using System.IO;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the rekordbox installation.
/// </summary>
public class RekordboxDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        var result = FindInstalledApplication.Find(
            providerName: "Rekordbox",
            executables:
            [
                "rekordbox.exe",
                "rekordboxAgent.exe"
            ],
            installPaths:
            [
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "rekordbox"),

                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "rekordbox")
            ]);

        // If rekordbox isn't installed there's nothing else to discover.
        if (!result.Installed)
        {
            return result;
        }

        var settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Pioneer",
            "rekordbox");

        result.SettingsPath = settingsFolder;

        var database = Path.Combine(settingsFolder, "master.db");

        if (File.Exists(database))
        {
            result.DatabasePath = database;
        }

        return result;
    }
}