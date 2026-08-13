using System;

namespace DJLibraryManager.UI.Models.Media;

/// <summary>
/// Represents the technical and integrity information discovered
/// by inspecting a physical media file.
///
/// This describes the actual file on disk rather than the metadata
/// stored by a DJ provider.
///
/// File inspection is used by Search when evaluating duplicate
/// candidates and must not modify the DIASISS library.
/// </summary>
public sealed class FileInspectionResult
{
    // ============================================================
    // File Identity
    // ============================================================

    /// <summary>
    /// Full path of the inspected file.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the file exists.
    /// </summary>
    public bool Exists { get; init; }

    /// <summary>
    /// Indicates whether the file could be opened and inspected.
    /// </summary>
    public bool IsReadable { get; init; }

    // ============================================================
    // Integrity
    // ============================================================

    /// <summary>
    /// Indicates whether the file passed the available integrity
    /// checks.
    ///
    /// A value of false means the file must not be recommended
    /// as the preferred duplicate.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Human-readable description of the integrity result.
    /// </summary>
    public string IntegrityStatus { get; init; } = string.Empty;

    // ============================================================
    // Technical Format
    // ============================================================

    /// <summary>
    /// Container or file format, for example MP3, FLAC or WAV.
    /// </summary>
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// Audio codec, when it can be determined.
    /// </summary>
    public string Codec { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the audio format is lossless.
    /// </summary>
    public bool? IsLossless { get; init; }

    // ============================================================
    // Audio Quality
    // ============================================================

    /// <summary>
    /// Bitrate in bits per second, when available.
    /// </summary>
    public int? Bitrate { get; init; }

    /// <summary>
    /// Sample rate in Hz, when available.
    /// </summary>
    public int? SampleRate { get; init; }

    /// <summary>
    /// Bits per sample, when available.
    /// </summary>
    public int? BitDepth { get; init; }

    /// <summary>
    /// Number of audio channels, when available.
    /// </summary>
    public int? Channels { get; init; }

    // ============================================================
    // Duration
    // ============================================================

    /// <summary>
    /// Duration reported by the physical media file.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    // ============================================================
    // Error Information
    // ============================================================

    /// <summary>
    /// Error encountered during inspection, if any.
    ///
    /// This is informational and should not contain sensitive
    /// system information beyond what is useful for diagnosis.
    /// </summary>
    public string? ErrorMessage { get; init; }
}