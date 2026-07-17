using System;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Serato DJ Pro installation.
/// </summary>
public class SeratoDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        return FindInstalledApplication.Find(
            providerName: "Serato",
            executables:
            [
                "Serato DJ Pro.exe",
                "Serato DJ Lite.exe"
            ],
            installPaths:
            [
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Serato"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Serato")
            ]);
    }
}