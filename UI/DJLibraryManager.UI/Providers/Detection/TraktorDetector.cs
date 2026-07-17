using System;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Traktor Pro installation.
/// </summary>
public class TraktorDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        return FindInstalledApplication.Find(
            providerName: "Traktor",
            executables:
            [
                "Traktor.exe"
            ],
            installPaths:
            [
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Native Instruments\Traktor Pro 4"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Native Instruments\Traktor Pro 3"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Native Instruments\Traktor Pro 4"),

                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Native Instruments\Traktor Pro 3")
            ]);
    }
}