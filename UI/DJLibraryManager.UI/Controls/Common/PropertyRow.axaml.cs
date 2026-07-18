using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Common;

public partial class PropertyRow : UserControl
{
    public PropertyRow()
    {
        InitializeComponent();
    }

    // --------------------------------------------------------------------
    // Icon
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<PropertyRow, string>(nameof(Icon));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // --------------------------------------------------------------------
    // Label
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<PropertyRow, string>(nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // --------------------------------------------------------------------
    // Value
    // --------------------------------------------------------------------

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<PropertyRow, string?>(nameof(Value));

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}