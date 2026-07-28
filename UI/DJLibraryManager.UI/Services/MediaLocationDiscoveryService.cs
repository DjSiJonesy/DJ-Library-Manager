using DJLibraryManager.Core.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Discovers likely media locations on the local machine.
///
/// This service intentionally performs a lightweight directory discovery.
/// It does not recursively scan the file system for media files.
/// </summary>
public sealed class MediaLocationDiscoveryService
{
    private readonly DriveDiscoveryService _driveDiscoveryService = new();

    private static readonly string[] CommonFolderNames =
    {
        "Music",
        "DJ Music",
        "DJ Library",
        "Media",
        "Audio",
        "Songs"
    };

    private static readonly HashSet<string> IgnoredUserProfiles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Default",
            "Default User",
            "Public",
            "All Users",
            "DefaultAppPool"
        };

    public IReadOnlyList<MediaLocation> DiscoverLocations()
    {
        var locations = new List<MediaLocation>();

        foreach (var drive in _driveDiscoveryService.DiscoverDrives())
        {
            if (!drive.Ready)
                continue;

            var driveRoot = drive.DriveLetter + Path.DirectorySeparatorChar;

            //
            // Check common folders in the root of the drive
            //
            foreach (var folder in CommonFolderNames)
            {
                AddLocation(locations, Path.Combine(driveRoot, folder));
            }

            //
            // Check Users\<Profile>\Music etc.
            //
            var usersFolder = Path.Combine(driveRoot, "Users");

            if (!Directory.Exists(usersFolder))
                continue;

            try
            {
                foreach (var userFolder in Directory.GetDirectories(usersFolder))
                {
                    var profileName = Path.GetFileName(userFolder);

                    if (IgnoredUserProfiles.Contains(profileName))
                        continue;

                    foreach (var folder in CommonFolderNames)
                    {
                        AddLocation(locations, Path.Combine(userFolder, folder));
                    }
                }
            }
            catch
            {
                // Ignore inaccessible folders
            }
        }

        return locations
            .GroupBy(l => l.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(l => l.Path)
            .ToList();
    }

    private static void AddLocation(
        ICollection<MediaLocation> locations,
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (!Directory.Exists(path))
            return;

        locations.Add(new MediaLocation
        {
            Name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)),
            Path = path,
            Drive = Path.GetPathRoot(path) ?? string.Empty,
            Exists = true,
            AutoDiscovered = true,
            DiscoveredOn = DateTime.Now
        });
    }
}