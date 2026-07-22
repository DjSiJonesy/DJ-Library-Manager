using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Common;

public partial class LocationCard : UserControl
{
    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<LocationCard, string>(nameof(Icon));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<LocationCard, string>(nameof(Title));

    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<LocationCard, string>(nameof(Path));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<LocationCard, ICommand?>(nameof(Command));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public LocationCard()
    {
        InitializeComponent();
    }
}