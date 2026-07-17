using Avalonia.Media.Imaging;

namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents a discovered DJ provider and its installation status.
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Display name of the provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the provider is installed on this computer.
    /// </summary>
    public bool Installed { get; set; }

    /// <summary>
    /// Detected provider version.
    /// Empty if the provider is not installed or the version is unknown.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Logo displayed by the ProviderCard.
    /// </summary>
    public Bitmap? ProviderLogo { get; set; }

    /// <summary>
    /// Status text displayed on the ProviderCard.
    /// </summary>
    public string Status => Installed
        ? "✓ Installed"
        : "Not Installed";
}