namespace DJLibraryManager.UI.Models.Analysis;

/// <summary>
/// Represents a single row within the Analysis table.
/// </summary>
public sealed class AnalysisCategoryInfo
{
    public string Name { get; set; } = string.Empty;

    public int IssueCount { get; set; }

    public double HealthScore { get; set; }
}