using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

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

    public static readonly StyledProperty<Bitmap?> ProviderLogoProperty =
        AvaloniaProperty.Register<ProviderCard, Bitmap?>(nameof(ProviderLogo));

    public static readonly StyledProperty<bool> InstalledProperty =
        AvaloniaProperty.Register<ProviderCard, bool>(
            nameof(Installed),
            true);

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

    public Bitmap? ProviderLogo
    {
        get => GetValue(ProviderLogoProperty);
        set => SetValue(ProviderLogoProperty, value);
    }

    public bool Installed
    {
        get => GetValue(InstalledProperty);
        set => SetValue(InstalledProperty, value);
    }
}