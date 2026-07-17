using DJLibraryManager.UI.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
        foreach (var installPath in installPaths)
        {
            if (!Directory.Exists(installPath))
            {
                continue;
            }

            foreach (var executable in executables)
            {
                var file = Directory
                    .EnumerateFiles(
                        installPath,
                        executable,
                        SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (file is null)
                {
                    continue;
                }

                string version = string.Empty;

                try
                {
                    version = FileVersionInfo
                        .GetVersionInfo(file)
                        .ProductVersion ?? string.Empty;
                }
                catch
                {
                    // Ignore version lookup failures.
                }

                return new ProviderDiscoveryResult
                {
                    Name = providerName,
                    Installed = true,
                    InstallPath = Path.GetDirectoryName(file),
                    ExecutablePath = file,
                    Version = version
                };
            }
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