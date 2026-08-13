using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DJLibraryManager.UI.Models.Media;
using TagLib;

namespace DJLibraryManager.UI.Services.Media;

/// <summary>
/// Inspects physical media files and returns technical and
/// integrity information about the actual file on disk.
///
/// This service does not modify the DIASISS library.
/// </summary>
public sealed class FileInspectionService
{
    /// <summary>
    /// Inspects a physical media file.
    /// </summary>
    public Task<FileInspectionResult> InspectAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(
            () => Inspect(
                filePath,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs the physical file inspection.
    /// </summary>
    private static FileInspectionResult Inspect(
        string filePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ========================================================
        // Existence
        // ========================================================

        if (!System.IO.File.Exists(filePath))
        {
            return new FileInspectionResult
            {
                FilePath = filePath,
                Exists = false,
                IsReadable = false,
                IsHealthy = false,
                IntegrityStatus = "File is missing."
            };
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ====================================================
            // Basic filesystem access
            // ====================================================

            var fileInfo =
                new FileInfo(filePath);

            if (fileInfo.Length <= 0)
            {
                return new FileInspectionResult
                {
                    FilePath = filePath,
                    Exists = true,
                    IsReadable = false,
                    IsHealthy = false,
                    IntegrityStatus = "File is empty."
                };
            }

            // ====================================================
            // TagLib inspection
            // ====================================================

            using var file =
                TagLib.File.Create(filePath);

            cancellationToken.ThrowIfCancellationRequested();

            var properties =
                file.Properties;

            if (properties is null)
            {
                return new FileInspectionResult
                {
                    FilePath = filePath,
                    Exists = true,
                    IsReadable = true,
                    IsHealthy = false,
                    IntegrityStatus =
                        "Audio properties could not be read."
                };
            }

            // ====================================================
            // Duration
            // ====================================================

            var duration =
                properties.Duration;

            if (duration <= TimeSpan.Zero)
            {
                return new FileInspectionResult
                {
                    FilePath = filePath,
                    Exists = true,
                    IsReadable = true,
                    IsHealthy = false,
                    IntegrityStatus =
                        "Audio duration could not be determined."
                };
            }

            // ====================================================
            // Technical information
            // ====================================================

            var format =
                Path.GetExtension(filePath)
                    .TrimStart('.')
                    .ToUpperInvariant();

            var firstCodec =
                properties.Codecs?
                    .FirstOrDefault();

            var codec =
                firstCodec?.Description
                ?? string.Empty;

            // TagLib# exposes these values as methods in the
            // installed version.

            var bitrateValue =
    properties.AudioBitrate;

            var bitrate =
                bitrateValue > 0
                    ? (int?)bitrateValue * 1000
                    : null;

            var sampleRateValue =
                properties.AudioSampleRate;

            var sampleRate =
                sampleRateValue > 0
                    ? (int?)sampleRateValue
                    : null;

            var channelsValue =
                properties.AudioChannels;

            var channels =
                channelsValue > 0
                    ? (int?)channelsValue
                    : null;

            var bitDepthValue =
                properties.BitsPerSample;

            var bitDepth =
                bitDepthValue > 0
                    ? (int?)bitDepthValue
                    : null;

            var isLossless =
                DetermineLossless(
                    format,
                    codec);

            // ====================================================
            // Successful inspection
            // ====================================================

            return new FileInspectionResult
            {
                FilePath = filePath,

                Exists = true,

                IsReadable = true,

                IsHealthy = true,

                IntegrityStatus =
                    "File opened and audio properties were read successfully.",

                Format = format,

                Codec = codec,

                IsLossless = isLossless,

                Bitrate = bitrate,

                SampleRate = sampleRate,

                BitDepth = bitDepth,

                Channels = channels,

                Duration = duration
            };
        }
        catch (CorruptFileException ex)
        {
            return new FileInspectionResult
            {
                FilePath = filePath,
                Exists = true,
                IsReadable = false,
                IsHealthy = false,
                IntegrityStatus =
                    "File appears to be corrupt.",
                ErrorMessage =
                    ex.Message
            };
        }
        catch (Exception ex)
        {
            return new FileInspectionResult
            {
                FilePath = filePath,
                Exists = true,
                IsReadable = false,
                IsHealthy = false,
                IntegrityStatus =
                    "File could not be inspected.",
                ErrorMessage =
                    ex.Message
            };
        }
    }

    /// <summary>
    /// Determines whether the detected format/codec is normally
    /// considered lossless.
    ///
    /// This is a classification used for recommendation scoring;
    /// it does not attempt to determine whether a lossy file was
    /// originally created from a lossless source.
    /// </summary>
    private static bool? DetermineLossless(
        string format,
        string codec)
    {
        if (string.IsNullOrWhiteSpace(format) &&
            string.IsNullOrWhiteSpace(codec))
        {
            return null;
        }

        if (format is
            "FLAC" or
            "WAV" or
            "AIFF" or
            "AIF" or
            "ALAC")
        {
            return true;
        }

        if (format is
            "MP3" or
            "AAC" or
            "M4A" or
            "OGG" or
            "OPUS")
        {
            return false;
        }

        if (codec.Contains(
                "FLAC",
                StringComparison.OrdinalIgnoreCase)
            ||
            codec.Contains(
                "ALAC",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return null;
    }
}