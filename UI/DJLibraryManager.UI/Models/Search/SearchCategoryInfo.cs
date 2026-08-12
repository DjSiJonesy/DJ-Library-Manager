using CommunityToolkit.Mvvm.ComponentModel;

namespace DJLibraryManager.UI.Models.Search;

/// <summary>
/// Represents a Search category displayed in the Search workspace.
/// </summary>
public partial class SearchCategoryInfo : ObservableObject
{
    /// <summary>
    /// Display name of the Search category.
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// Explains what this Search category investigates.
    /// </summary>
    [ObservableProperty]
    private string description = string.Empty;

    /// <summary>
    /// Number of Analysis issues currently available
    /// for this Search category.
    /// </summary>
    [ObservableProperty]
    private int issueCount;
}