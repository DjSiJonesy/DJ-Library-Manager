using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Controls.Cards;

public partial class ProviderCard : UserControl
{
    private IBrush? _normalBorder;

    private static readonly IBrush HoverBorder =
        new SolidColorBrush(Color.Parse("#ab58fa"));

    public ProviderCard()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            _normalBorder = CardBorder.BorderBrush;
        };
    }

    private void CardBorder_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is not ProviderInfo { Installed: true })
            return;

        CardBorder.BorderBrush = HoverBorder;
    }

    private void CardBorder_PointerExited(object? sender, PointerEventArgs e)
    {
        if (_normalBorder is not null)
        {
            CardBorder.BorderBrush = _normalBorder;
        }
    }
}