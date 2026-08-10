using System;

namespace DJLibraryManager.UI.Models.Discovery;

/// <summary>
/// Persisted validation result for a discovered media location.
/// </summary>
public sealed class DiscoveryValidationRecord
{
    /// <summary>
    /// Media location this record belongs to.
    /// </summary>
    public required string LocationPath { get; init; }

    /// <summary>
    /// When validation was last performed.
    /// </summary>
    public DateTime LastValidated { get; set; }

    /// <summary>
    /// True when changes were detected.
    /// </summary>
    public bool HasChanges { get; set; }
}