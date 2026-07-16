using Avalonia.Media.Imaging;

namespace DJLibraryManager.UI.Models;

public class ProviderInfo
{
    public string Name { get; set; } = string.Empty;

    public bool Installed { get; set; }

    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Logo displayed by the ProviderCard.
    /// </summary>
    public Bitmap? ProviderLogo { get; set; }

    public string Status => Installed
        ? "✓ Installed"
        : "Not Installed";
}