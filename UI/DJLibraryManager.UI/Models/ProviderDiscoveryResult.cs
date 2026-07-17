namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents the discovery result for a DJ software provider.
/// </summary>
public class ProviderDiscoveryResult
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
    /// Full path to the executable.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// Primary database used by the provider.
    /// </summary>
    public string? DatabasePath { get; set; }

    /// <summary>
    /// Provider settings or configuration folder.
    /// </summary>
    public string? SettingsPath { get; set; }

    /// <summary>
    /// Default music library location used by the provider.
    /// </summary>
    public string? MusicLibraryPath { get; set; }
}