using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents DIASISS's analysis of one metadata field across
/// independently validated provider evidence.
/// </summary>
public sealed class MetadataConsensusResult
{
    /// <summary>
    /// Metadata field being analysed.
    ///
    /// Examples:
    /// Artist, Title, Album, Genre, Year, BPM, Duration.
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Consensus value when the evidence supports one.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Number of independent provider candidates supporting
    /// the consensus value.
    /// </summary>
    public int SupportingProviders { get; init; }

    /// <summary>
    /// Total number of provider candidates that supplied a value
    /// for this field.
    /// </summary>
    public int ProvidersWithValue { get; init; }

    /// <summary>
    /// Percentage of providers supporting the consensus value.
    /// </summary>
    public double AgreementPercentage { get; init; }

    /// <summary>
    /// Indicates how strongly the available evidence agrees.
    /// </summary>
    public MetadataConsensusStrength Strength { get; init; }

    /// <summary>
    /// Provider names supporting the consensus value.
    /// </summary>
    public IReadOnlyList<string> SupportingSources { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Provider names that supplied conflicting values.
    /// </summary>
    public IReadOnlyList<string> ConflictingSources { get; init; }
        = Array.Empty<string>();
}

/// <summary>
/// Strength of agreement between independent metadata sources.
/// </summary>
public enum MetadataConsensusStrength
{
    NoData,

    Conflict,

    Weak,

    Moderate,

    Strong
}