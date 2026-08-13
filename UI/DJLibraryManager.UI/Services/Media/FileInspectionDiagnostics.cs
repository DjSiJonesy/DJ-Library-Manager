using System;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services.Media;

/// <summary>
/// Temporary diagnostic helper used to verify the physical
/// file inspection service against a real audio file.
///
/// This is temporary development code and should be removed
/// once the inspection layer has been validated.
/// </summary>
public static class FileInspectionDiagnostics
{
    public static async Task RunTestAsync()
    {
        const string testFile =
            @"C:\Users\Simon\Music\Downloads\Amazon_Music\11. Ain't No Mountain High Enough (Jax Jones Remix) Cascada.mp3";

        var service =
            new FileInspectionService();

        var result =
            await service.InspectAsync(testFile);

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            "DIASISS FILE INSPECTION TEST");

        Console.WriteLine(
            "================================================");

        Console.WriteLine(
            $"File:          {result.FilePath}");

        Console.WriteLine(
            $"Exists:        {result.Exists}");

        Console.WriteLine(
            $"Readable:      {result.IsReadable}");

        Console.WriteLine(
            $"Healthy:       {result.IsHealthy}");

        Console.WriteLine(
            $"Integrity:     {result.IntegrityStatus}");

        Console.WriteLine(
            $"Format:        {result.Format}");

        Console.WriteLine(
            $"Codec:         {result.Codec}");

        Console.WriteLine(
            $"Lossless:      {result.IsLossless}");

        Console.WriteLine(
            $"Bitrate:       {FormatBitrate(result.Bitrate)}");

        Console.WriteLine(
            $"Sample Rate:   {FormatSampleRate(result.SampleRate)}");

        Console.WriteLine(
            $"Bit Depth:     {FormatBitDepth(result.BitDepth)}");

        Console.WriteLine(
            $"Channels:      {result.Channels?.ToString() ?? "Unknown"}");

        Console.WriteLine(
            $"Duration:      {result.Duration?.ToString(@"hh\:mm\:ss") ?? "Unknown"}");

        Console.WriteLine(
            $"Error:         {result.ErrorMessage ?? "None"}");

        Console.WriteLine(
            "================================================");
    }

    private static string FormatBitrate(int? bitrate)
    {
        if (!bitrate.HasValue)
            return "Unknown";

        return $"{bitrate.Value / 1000:N0} kbps";
    }

    private static string FormatSampleRate(int? sampleRate)
    {
        if (!sampleRate.HasValue)
            return "Unknown";

        return $"{sampleRate.Value:N0} Hz";
    }

    private static string FormatBitDepth(int? bitDepth)
    {
        if (!bitDepth.HasValue)
            return "Unknown";

        return $"{bitDepth.Value}-bit";
    }
}