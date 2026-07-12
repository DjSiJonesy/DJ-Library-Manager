namespace DJLibraryManager.UI.Models;

public class ProviderInfo
{
    public string Name { get; set; } = string.Empty;

    public bool Installed { get; set; }

    public string Version { get; set; } = string.Empty;

    public string Status => Installed
        ? "✓ Installed"
        : "Not Installed";
}