using CommunityToolkit.Mvvm.ComponentModel;

namespace DJLibraryManager.UI.Models.Improve;

/// <summary>
/// Describes an Improve workspace category.
/// </summary>
public partial class ImproveCategoryInfo : ObservableObject
{
    // ============================================================
    // Name
    // ============================================================

    /// <summary>
    /// Display name of the Improve category.
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    // ============================================================
    // Icon
    // ============================================================

    /// <summary>
    /// Icon displayed on the category button.
    /// </summary>
    [ObservableProperty]
    private string icon = string.Empty;

    // ============================================================
    // Description
    // ============================================================

    /// <summary>
    /// Description displayed when this category is selected.
    /// </summary>
    [ObservableProperty]
    private string description = string.Empty;

    // ============================================================
    // Count
    // ============================================================

    /// <summary>
    /// Number of issues currently associated with this category.
    ///
    /// The value is populated from the existing Search state.
    /// Improve does not perform a new search.
    /// </summary>
    [ObservableProperty]
    private int count;
}