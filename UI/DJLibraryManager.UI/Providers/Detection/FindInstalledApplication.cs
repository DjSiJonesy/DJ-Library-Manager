using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Providers.Detection;

/// <summary>
/// Searches for an installed DJ application.
/// </summary>
public static class FindInstalledApplication
{
    public static ProviderDiscoveryResult Find(
        string providerName,
        string[] executables,
        string[] installPaths)
    {
        var candidates = new List<(string File, Version Version, string VersionText)>();

        foreach (var installPath in installPaths)
        {
            if (!Directory.Exists(installPath))
            {
                continue;
            }

            foreach (var executable in executables)
            {
                foreach (var file in Directory.EnumerateFiles(
                    installPath,
                    executable,
                    SearchOption.AllDirectories))
                {
                    string versionText = string.Empty;
                    Version version = new(0, 0, 0, 0);

                    try
                    {
                        versionText = FileVersionInfo
                            .GetVersionInfo(file)
                            .ProductVersion ?? string.Empty;

                        Version.TryParse(versionText, out var parsedVersion);

                        if (parsedVersion is not null)
                        {
                            version = parsedVersion;
                        }
                    }
                    catch
                    {
                        // Ignore version lookup failures.
                    }

                    candidates.Add((file, version, versionText));
                }
            }
        }

        var bestMatch = candidates
            .OrderByDescending(c => c.Version)
            .FirstOrDefault();

        if (bestMatch.File is not null)
        {
            return new ProviderDiscoveryResult
            {
                Name = providerName,
                Installed = true,
                InstallPath = Path.GetDirectoryName(bestMatch.File),
                ExecutablePath = bestMatch.File,
                Version = bestMatch.VersionText
            };
        }

        return new ProviderDiscoveryResult
        {
            Name = providerName,
            Installed = false,
            InstallPath = null,
            ExecutablePath = null,
            Version = string.Empty
        };
    }
}