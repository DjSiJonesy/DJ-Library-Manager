using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DJLibraryManager.UI.Controls.Common;

public partial class PropertyRow : UserControl
{
    public PropertyRow()
    {
        InitializeComponent();
    }

    // --------------------------------------------------------------------
    // Status Indicator
    // --------------------------------------------------------------------

    public static readonly StyledProperty<bool> ShowIndicatorProperty =
        AvaloniaProperty.Register<PropertyRow, bool>(
            nameof(ShowIndicator));

    public bool ShowIndicator
    {
        get => GetValue(ShowIndicatorProperty);
        set => SetValue(ShowIndicatorProperty, value);
    }

    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<PropertyRow, IBrush?>(
            nameof(IndicatorBrush));

    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    // --------------------------------------------------------------------
    // Icon Visibility
    // --------------------------------------------------------------------

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<PropertyRow, bool>(
            nameof(ShowIcon),
            defaultValue: true);

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    // --------------------------------------------------------------------
    // Icon
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<PropertyRow, string>(
            nameof(Icon));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // --------------------------------------------------------------------
    // Label
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<PropertyRow, string>(
            nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // --------------------------------------------------------------------
    // Value
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<PropertyRow, string?>(
            nameof(Value));

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}