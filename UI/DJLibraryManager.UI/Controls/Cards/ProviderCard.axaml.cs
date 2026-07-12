using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Cards;

public partial class ProviderCard : UserControl
{
    public ProviderCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> ProviderNameProperty =
        AvaloniaProperty.Register<ProviderCard, string>(nameof(ProviderName));

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<ProviderCard, string>(nameof(Status));

    public string ProviderName
    {
        get => GetValue(ProviderNameProperty);
        set => SetValue(ProviderNameProperty, value);
    }

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Returns the logo for the provider.
    /// </summary>
    public string LogoPath =>
        ProviderName switch
        {
            "VirtualDJ" => "/Assets/Providers/VirtualDJ.png",
            "Rekordbox" => "/Assets/Providers/Rekordbox.png",
            "Serato" => "/Assets/Providers/Serato.png",
            "Engine DJ" => "/Assets/Providers/EngineDJ.png",
            "Traktor" => "/Assets/Providers/Traktor.png",
            "djay" => "/Assets/Providers/djay.png",
            _ => "/Assets/Providers/Unknown.png"
        };
}