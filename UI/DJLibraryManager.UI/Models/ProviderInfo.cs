using Avalonia.Media.Imaging;

namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents a DJ software provider displayed by the UI.
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Provider display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the provider is installed.
    /// </summary>
    public bool Installed { get; set; }

    /// <summary>
    /// Installed application version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Installation folder.
    /// </summary>
    public string? InstallPath { get; set; }

    /// <summary>
    /// Full path to the provider executable.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// Provider database path.
    /// </summary>
    public string? DatabasePath { get; set; }

    /// <summary>
    /// Provider settings folder.
    /// </summary>
    public string? SettingsPath { get; set; }

    /// <summary>
    /// Provider logo displayed by the dashboard.
    /// </summary>
    public Bitmap? ProviderLogo { get; set; }

    /// <summary>
    /// Friendly installation status.
    /// </summary>
    public string Status => Installed
        ? "✓ Installed"
        : "Not Installed";
}