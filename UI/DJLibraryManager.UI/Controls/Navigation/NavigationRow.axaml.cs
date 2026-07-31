using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Navigation;

public partial class NavigationRow : UserControl
{
    public NavigationRow()
    {
        InitializeComponent();
    }

    // =====================================================
    // Icon
    // =====================================================

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<NavigationRow, string>(
            nameof(Icon),
            string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // =====================================================
    // Text
    // =====================================================

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NavigationRow, string>(
            nameof(Text),
            string.Empty);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // =====================================================
    // Left Command
    // =====================================================

    public static readonly StyledProperty<ICommand?> LeftCommandProperty =
        AvaloniaProperty.Register<NavigationRow, ICommand?>(
            nameof(LeftCommand));

    public ICommand? LeftCommand
    {
        get => GetValue(LeftCommandProperty);
        set => SetValue(LeftCommandProperty, value);
    }

    public static readonly StyledProperty<object?> LeftCommandParameterProperty =
        AvaloniaProperty.Register<NavigationRow, object?>(
            nameof(LeftCommandParameter));

    public object? LeftCommandParameter
    {
        get => GetValue(LeftCommandParameterProperty);
        set => SetValue(LeftCommandParameterProperty, value);
    }

    // =====================================================
    // Right Command
    // =====================================================

    public static readonly StyledProperty<ICommand?> RightCommandProperty =
        AvaloniaProperty.Register<NavigationRow, ICommand?>(
            nameof(RightCommand));

    public ICommand? RightCommand
    {
        get => GetValue(RightCommandProperty);
        set => SetValue(RightCommandProperty, value);
    }

    public static readonly StyledProperty<object?> RightCommandParameterProperty =
        AvaloniaProperty.Register<NavigationRow, object?>(
            nameof(RightCommandParameter));

    public object? RightCommandParameter
    {
        get => GetValue(RightCommandParameterProperty);
        set => SetValue(RightCommandParameterProperty, value);
    }
}