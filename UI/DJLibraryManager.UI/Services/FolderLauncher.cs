using System;
using System.Diagnostics;
using System.IO;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Provides helper methods for opening folders in Windows Explorer.
/// </summary>
public static class FolderLauncher
{
    /// <summary>
    /// Opens the specified folder in Windows Explorer.
    /// </summary>
    /// <param name="path">
    /// The folder to open.
    /// </param>
    /// <returns>
    /// True if Explorer was launched; otherwise false.
    /// </returns>
    public static bool Open(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            // If the path is already a directory, open it.
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true
                });

                return true;
            }

            // If the path is a file, open the containing folder.
            if (File.Exists(path))
            {
                var folder = Path.GetDirectoryName(path);

                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{folder}\"",
                        UseShellExecute = true
                    });

                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}