using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DJLibraryManager.UI.Controls.Common;

public partial class HealthBar : UserControl
{
    public HealthBar()
    {
        InitializeComponent();

        UpdateHealthBrush();
    }

    // ============================================================
    // Health Value
    // ============================================================

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

    // ============================================================
    // Health Colour
    // ============================================================

    public static readonly DirectProperty<HealthBar, IBrush> HealthBrushProperty =
        AvaloniaProperty.RegisterDirect<HealthBar, IBrush>(
            nameof(HealthBrush),
            control => control.HealthBrush);

    private IBrush _healthBrush = Brushes.LimeGreen;

    public IBrush HealthBrush
    {
        get => _healthBrush;
        private set => SetAndRaise(
            HealthBrushProperty,
            ref _healthBrush,
            value);
    }

    // ============================================================
    // Property Changes
    // ============================================================

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            UpdateHealthBrush();
        }
    }

    // ============================================================
    // Health Colour
    // ============================================================

    private void UpdateHealthBrush()
    {
        HealthBrush = Value switch
        {
            <= 50 => Brushes.Red,
            <= 90 => Brushes.Goldenrod,
            _ => Brushes.LimeGreen
        };
    }
}