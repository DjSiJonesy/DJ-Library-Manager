using DJLibraryManager.Core.Services;
using DJLibraryManager.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DJLibraryManager.UI.Services;

public class MediaLibraryDiscoveryService
{
    private readonly DriveDiscoveryService _driveDiscoveryService = new();

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".flac",
        ".aac",
        ".m4a",
        ".aif",
        ".aiff",
        ".ogg",
        ".wma"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".m4v",
        ".mov",
        ".avi",
        ".mkv",
        ".wmv",
        ".mpeg",
        ".mpg",
        ".webm"
    };

    private static readonly HashSet<string> IgnoredFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Windows",
        "Program Files",
        "Program Files (x86)",
        "ProgramData",
        "$Recycle.Bin",
        "System Volume Information"
    };

    public List<MediaLibrary> DiscoverLibraries()
    {
        var libraries = new List<MediaLibrary>();

        foreach (var drive in _driveDiscoveryService.DiscoverDrives())
        {
            var root = drive.DriveLetter;

            if (!Directory.Exists(root))
                continue;

            ScanFolder(root, libraries);
        }

        return libraries
            .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.Path)
            .ToList();
    }

    public List<MediaLibrary> DiscoverLibraries(MediaLocation location)
    {
        var libraries = new List<MediaLibrary>();

        if (!Directory.Exists(location.Path))
            return libraries;

        ScanFolder(location.Path, libraries);

        return libraries
            .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.Path)
            .ToList();
    }

    private void ScanFolder(string path, List<MediaLibrary> libraries)
    {
        try
        {
            var audioFiles = 0;
            var videoFiles = 0;
            long totalBytes = 0;

            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    var extension = Path.GetExtension(file);

                    if (AudioExtensions.Contains(extension))
                    {
                        audioFiles++;
                        totalBytes += new FileInfo(file).Length;
                    }
                    else if (VideoExtensions.Contains(extension))
                    {
                        videoFiles++;
                        totalBytes += new FileInfo(file).Length;
                    }
                }
                catch
                {
                    // Ignore unreadable files.
                }
            }

            if (audioFiles > 0 || videoFiles > 0)
            {
                libraries.Add(new MediaLibrary
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                    Drive = Path.GetPathRoot(path) ?? string.Empty,
                    AudioFileCount = audioFiles,
                    VideoFileCount = videoFiles,
                    TotalSizeBytes = totalBytes,
                    IsLibraryRoot = true
                });
            }

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                var directoryInfo = new DirectoryInfo(directory);

                if (IgnoredFolders.Contains(directoryInfo.Name))
                    continue;

                if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                    continue;

                ScanFolder(directory, libraries);
            }
        }
        catch
        {
            // Ignore folders we cannot access.
        }
    }
}