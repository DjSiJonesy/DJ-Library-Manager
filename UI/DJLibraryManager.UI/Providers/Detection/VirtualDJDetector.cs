using System;
using System.IO;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the VirtualDJ installation.
/// </summary>
public class VirtualDJDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        var result = FindInstalledApplication.Find(
            providerName: "VirtualDJ",
            executables:
            [
                "virtualdj.exe"
            ],
            installPaths:
            [
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "VirtualDJ"),

                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "VirtualDJ")
            ]);

        // If VirtualDJ isn't installed there's nothing else to discover.
        if (!result.Installed)
        {
            return result;
        }

        var settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VirtualDJ");

        result.SettingsPath = settingsFolder;

        var database = Path.Combine(settingsFolder, "database.xml");

        if (File.Exists(database))
        {
            result.DatabasePath = database;
        }

        return result;
    }
}