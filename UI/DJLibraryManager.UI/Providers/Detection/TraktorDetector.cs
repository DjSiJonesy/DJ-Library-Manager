using System;
using System.IO;
using System.Linq;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Detects the Traktor Pro installation.
/// </summary>
public class TraktorDetector : IProviderDetector
{
    public ProviderDiscoveryResult Discover()
    {
        var result = FindInstalledApplication.Find(
            providerName: "Traktor",
            executables:
            [
                "Traktor.exe"
            ],
            installPaths:
            [
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Native Instruments"),

                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Native Instruments")
            ]);

        if (!result.Installed)
        {
            return result;
        }

        var nativeInstrumentsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Native Instruments");

        if (!Directory.Exists(nativeInstrumentsFolder))
        {
            return result;
        }

        var traktorFolder = Directory
            .GetDirectories(nativeInstrumentsFolder, "Traktor*")
            .OrderByDescending(Path.GetFileName)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(traktorFolder))
        {
            return result;
        }

        result.SettingsPath = traktorFolder;

        var collectionFile = Path.Combine(
            traktorFolder,
            "collection.nml");

        if (File.Exists(collectionFile))
        {
            result.DatabasePath = collectionFile;
        }

        return result;
    }
}