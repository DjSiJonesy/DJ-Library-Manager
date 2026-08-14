using System;
using System.IO;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Provides centralised protection rules for filesystem paths
/// that must not be treated as part of the active DIASISS
/// library.
///
/// Currently, any file located beneath a directory whose name
/// contains "backup", case-insensitively, is considered protected.
///
/// The filename itself is deliberately not inspected.
/// </summary>
public static class ProtectedPathService
{
    // ============================================================
    // Backup Detection
    // ============================================================

    /// <summary>
    /// Determines whether a file is located inside a protected
    /// Backup directory.
    ///
    /// Directory names are checked case-insensitively.
    ///
    /// Examples that are protected:
    ///
    ///     C:\Music\Backup\Track.mp3
    ///     C:\Music\Backups\Track.mp3
    ///     C:\Music\DJ_Backup\Track.mp3
    ///     C:\Music\DJ Library Backup\Track.mp3
    ///     C:\Music\BackupOldMusic\Track.mp3
    ///
    /// A filename containing "Backup" is not itself protected:
    ///
    ///     C:\Music\Artist\Backup.mp3
    /// </summary>
    public static bool IsInsideBackupFolder(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var directory =
                Path.GetDirectoryName(filePath);

            while (!string.IsNullOrWhiteSpace(directory))
            {
                var directoryName =
                    Path.GetFileName(
                        directory.TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar));

                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    directoryName.Contains(
                        "backup",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var parent =
                    Directory.GetParent(directory);

                if (parent is null)
                    break;

                directory =
                    parent.FullName;
            }
        }
        catch
        {
            // If the path cannot be inspected, do not classify
            // it as protected here.
            //
            // Other library validation and analysis can still
            // report problems with the path.
        }

        return false;
    }
}