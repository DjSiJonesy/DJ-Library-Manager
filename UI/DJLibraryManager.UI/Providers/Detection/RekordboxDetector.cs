using System;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the rekordbox installation.
/// </summary>
public class RekordboxDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        return FindInstalledApplication.Find(
            providerName: "Rekordbox",
            executables:
            [
                "rekordbox.exe",
                "rekordboxAgent.exe"
            ],
            installPaths:
            [
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "rekordbox"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "rekordbox")
            ]);
    }
}