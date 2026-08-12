namespace DJLibraryManager.UI.Models.Analysis;

/// <summary>
/// Represents a single row within the Analysis table.
/// </summary>
public sealed class AnalysisCategoryInfo
{
    /// <summary>
    /// Name of the analysis category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Explains what the analysis category checks.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of issues identified by this category.
    /// </summary>
    public int IssueCount { get; set; }

    /// <summary>
    /// Health score for this category.
    /// </summary>
    public double HealthScore { get; set; }
}