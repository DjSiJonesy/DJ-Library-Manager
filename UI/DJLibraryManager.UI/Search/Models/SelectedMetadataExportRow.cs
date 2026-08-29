using DJLibraryManager.UI.Search.Models;

public sealed class SelectedMetadataExportRow
{
    public string FilePath { get; init; } = string.Empty;

    public string Artist { get; init; } = string.Empty;

    public string TrackTitle { get; init; } = string.Empty;

    public string Album { get; init; } = string.Empty;

    public string Genre { get; init; } = string.Empty;

    public string Field { get; init; } = string.Empty;

    public string CurrentValue { get; init; } = string.Empty;

    public string RecommendedValue { get; init; } = string.Empty;

    public double AgreementPercentage { get; init; }

    public int SupportingProviders { get; init; }

    public int ProvidersWithValue { get; init; }

    public MetadataConsensusStrength Strength { get; init; }

    public bool IsUserModified { get; init; }

    public string Reason { get; init; } = string.Empty;
}