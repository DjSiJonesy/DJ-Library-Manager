namespace DJLibraryManager.UI.Models;

/// <summary>
/// Represents the discovery result for a DJ software provider.
/// </summary>
public class ProviderDiscoveryResult
{
    public string Name { get; set; } = string.Empty;

    public bool Installed { get; set; }

    public string Version { get; set; } = string.Empty;

    public string? InstallPath { get; set; }

    public string? ExecutablePath { get; set; }
}