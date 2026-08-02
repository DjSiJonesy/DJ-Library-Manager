using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Common;

public partial class HealthBar : UserControl
{
    public HealthBar()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Health percentage (0-100).
    /// </summary>
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<HealthBar, int>(
            nameof(Value),
            100);

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}