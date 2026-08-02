using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Cards;

public partial class LibrarySummaryCard : UserControl
{
    public LibrarySummaryCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> MediaLocationTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(MediaLocationText), string.Empty);

    public string MediaLocationText
    {
        get => GetValue(MediaLocationTextProperty);
        set => SetValue(MediaLocationTextProperty, value);
    }

    public static readonly StyledProperty<string> LibraryCountTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(LibraryCountText), string.Empty);

    public string LibraryCountText
    {
        get => GetValue(LibraryCountTextProperty);
        set => SetValue(LibraryCountTextProperty, value);
    }

    public static readonly StyledProperty<string> AudioCountTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(AudioCountText), string.Empty);

    public string AudioCountText
    {
        get => GetValue(AudioCountTextProperty);
        set => SetValue(AudioCountTextProperty, value);
    }

    public static readonly StyledProperty<string> VideoCountTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(VideoCountText), string.Empty);

    public string VideoCountText
    {
        get => GetValue(VideoCountTextProperty);
        set => SetValue(VideoCountTextProperty, value);
    }

    public static readonly StyledProperty<string> TotalMediaTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(TotalMediaText), string.Empty);

    public string TotalMediaText
    {
        get => GetValue(TotalMediaTextProperty);
        set => SetValue(TotalMediaTextProperty, value);
    }

    public static readonly StyledProperty<string> TotalSizeTextProperty =
        AvaloniaProperty.Register<LibrarySummaryCard, string>(
            nameof(TotalSizeText), string.Empty);

    public string TotalSizeText
    {
        get => GetValue(TotalSizeTextProperty);
        set => SetValue(TotalSizeTextProperty, value);
    }
}