using System.IO;

namespace DJLibraryManager.Core.Models;

public class LibraryDrive
{
    public string DriveLetter { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public DriveType DriveType { get; set; }

    public string FileSystem { get; set; } = string.Empty;

    public double SizeGB { get; set; }

    public double FreeSpaceGB { get; set; }

    public bool Ready { get; set; }

    public string Role { get; set; } = "Unknown";
}