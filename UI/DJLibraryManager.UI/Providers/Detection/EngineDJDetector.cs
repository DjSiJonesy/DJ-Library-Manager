using System;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Engine DJ installation.
/// </summary>
public class EngineDJDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        return FindInstalledApplication.Find(
            providerName: "EngineDJ",
            executables:
            [
                "Engine DJ.exe"
            ],
            installPaths:
            [
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Engine DJ"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Engine DJ")
            ]);
    }
}