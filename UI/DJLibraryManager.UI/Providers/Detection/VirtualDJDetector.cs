using System;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the VirtualDJ installation.
/// </summary>
public class VirtualDJDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        return FindInstalledApplication.Find(
            providerName: "VirtualDJ",
            executables:
            [
                "virtualdj.exe"
            ],
            installPaths:
            [
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "VirtualDJ"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "VirtualDJ")
            ]);
    }
}