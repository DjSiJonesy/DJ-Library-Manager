using System;
using System.Collections.Generic;
using System.IO;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Provides a central definition of the media file types
/// supported by DIASISS.
/// </summary>
public static class MediaFileTypes
{
    private static readonly HashSet<string> AudioExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".wav",
            ".flac",
            ".aiff",
            ".aif",
            ".m4a",
            ".aac",
            ".ogg",
            ".wma"
        };

    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".mov",
            ".mkv",
            ".avi",
            ".mpeg",
            ".mpg"
        };

    /// <summary>
    /// Returns true if the specified file is a supported media type.
    /// </summary>
    public static bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath);

        return AudioExtensions.Contains(extension)
            || VideoExtensions.Contains(extension);
    }

    /// <summary>
    /// Returns true if the file is an audio format.
    /// </summary>
    public static bool IsAudio(string filePath)
    {
        return AudioExtensions.Contains(
            Path.GetExtension(filePath));
    }

    /// <summary>
    /// Returns true if the file is a video format.
    /// </summary>
    public static bool IsVideo(string filePath)
    {
        return VideoExtensions.Contains(
            Path.GetExtension(filePath));
    }
}