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

        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}