using System;
using System.IO;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Engine DJ installation.
/// </summary>
public class EngineDJDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        var result = FindInstalledApplication.Find(
            providerName: "EngineDJ",
            executables:
            [
                "Engine DJ.exe"
            ],
            installPaths:
            [
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Engine DJ"),

                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Engine DJ")
            ]);

        // If Engine DJ isn't installed there's nothing else to discover.
        if (!result.Installed)
        {
            return result;
        }

        var libraryFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "Engine Library");

        if (Directory.Exists(libraryFolder))
        {
            result.SettingsPath = libraryFolder;

            var databaseFolder = Path.Combine(
                libraryFolder,
                "Database2");

            if (Directory.Exists(databaseFolder))
            {
                result.DatabasePath = databaseFolder;
            }
        }

        return result;
    }
}