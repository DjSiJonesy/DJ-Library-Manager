using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.Controls.Rows;

public partial class MediaFolderRow : UserControl
{
    public MediaFolderRow()
    {
        InitializeComponent();
    }

    // =====================================================
    // Folder
    // =====================================================

    public static readonly StyledProperty<string> FolderProperty =
        AvaloniaProperty.Register<MediaFolderRow, string>(
            nameof(Folder),
            string.Empty);

    public string Folder
    {
        get => GetValue(FolderProperty);
        set => SetValue(FolderProperty, value);
    }

    // =====================================================
    // Audio
    // =====================================================

    public static readonly StyledProperty<int> AudioCountProperty =
        AvaloniaProperty.Register<MediaFolderRow, int>(
            nameof(AudioCount));

    public int AudioCount
    {
        get => GetValue(AudioCountProperty);
        set => SetValue(AudioCountProperty, value);
    }

    // =====================================================
    // Video
    // =====================================================

    public static readonly StyledProperty<int> VideoCountProperty =
        AvaloniaProperty.Register<MediaFolderRow, int>(
            nameof(VideoCount));

    public int VideoCount
    {
        get => GetValue(VideoCountProperty);
        set => SetValue(VideoCountProperty, value);
    }

    // =====================================================
    // Total
    // =====================================================

    public static readonly StyledProperty<int> TotalCountProperty =
        AvaloniaProperty.Register<MediaFolderRow, int>(
            nameof(TotalCount));

    public int TotalCount
    {
        get => GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    // =====================================================
    // Open Folder
    // =====================================================

    private void FolderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(Folder))
        {
            FolderLauncher.Open(Folder);
        }
    }
}