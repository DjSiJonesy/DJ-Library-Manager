using System;
using DJLibraryManager.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DJLibraryManager.Core.Services;

public class DriveDiscoveryService
{
    public IReadOnlyList<LibraryDrive> DiscoverDrives()
    {
        return DriveInfo.GetDrives()
            .Where(d =>
                d.IsReady &&
                (d.DriveType == DriveType.Fixed ||
                 d.DriveType == DriveType.Removable))
            .Select(d => new LibraryDrive
            {
                DriveLetter = d.Name.TrimEnd('\\'),
                Label = d.VolumeLabel,
                DriveType = d.DriveType,
                FileSystem = d.DriveFormat,
                SizeGB = Math.Round(d.TotalSize / 1073741824d, 2),
                FreeSpaceGB = Math.Round(d.TotalFreeSpace / 1073741824d, 2),
                Ready = true,
                Role = "Unknown"
            })
            .OrderBy(d => d.DriveLetter)
            .ToList();
    }
}