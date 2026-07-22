using System;
using System.Collections.Generic;
using DJLibraryManager.UI.Models.Media;

namespace DJLibraryManager.UI.Models.Import;


public sealed class ImportResult
{
    public bool Success { get; init; }

    public string ProviderName { get; init; } = string.Empty;

    public int TrackCount { get; init; }

    public int PlaylistCount { get; init; }

    public DateTime ImportedAt { get; init; }

    public IReadOnlyList<DJLMMediaItem> MediaItems { get; init; }
        = Array.Empty<DJLMMediaItem>();

    public string? ErrorMessage { get; init; }
}