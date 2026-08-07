using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaLocationRow : UserControl
{
    public WorkflowMediaLocationRow()
    {
        InitializeComponent();
    }

    // ============================================================
    // Drive
    // ============================================================

    public static readonly StyledProperty<string> DriveProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, string>(
            nameof(Drive),
            string.Empty);

    public string Drive
    {
        get => GetValue(DriveProperty);
        set => SetValue(DriveProperty, value);
    }

    // ============================================================
    // Path
    // ============================================================

    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, string>(
            nameof(Path),
            string.Empty);

    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    // ============================================================
    // Folder Count
    // ============================================================

    public static readonly StyledProperty<int> FolderCountProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, int>(
            nameof(FolderCount));

    public int FolderCount
    {
        get => GetValue(FolderCountProperty);
        set => SetValue(FolderCountProperty, value);
    }

    // ============================================================
    // Audio File Count
    // ============================================================

    public static readonly StyledProperty<int> AudioFileCountProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, int>(
            nameof(AudioFileCount));

    public int AudioFileCount
    {
        get => GetValue(AudioFileCountProperty);
        set => SetValue(AudioFileCountProperty, value);
    }

    // ============================================================
    // Video File Count
    // ============================================================

    public static readonly StyledProperty<int> VideoFileCountProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, int>(
            nameof(VideoFileCount));

    public int VideoFileCount
    {
        get => GetValue(VideoFileCountProperty);
        set => SetValue(VideoFileCountProperty, value);
    }

    // ============================================================
    // Status
    // ============================================================

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, string>(
            nameof(Status),
            "Ready to Discover");

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    // ============================================================
    // Status Brush
    // ============================================================

    public static readonly StyledProperty<IBrush?> StatusBrushProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, IBrush?>(
            nameof(StatusBrush));

    public IBrush? StatusBrush
    {
        get => GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    // ============================================================
    // Can Discover
    // ============================================================

    public static readonly StyledProperty<bool> CanDiscoverProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, bool>(
            nameof(CanDiscover));

    public bool CanDiscover
    {
        get => GetValue(CanDiscoverProperty);
        set => SetValue(CanDiscoverProperty, value);
    }

    // ============================================================
    // Can View
    // ============================================================

    public static readonly StyledProperty<bool> CanViewProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, bool>(
            nameof(CanView));

    public bool CanView
    {
        get => GetValue(CanViewProperty);
        set => SetValue(CanViewProperty, value);
    }

    // ============================================================
    // Discover Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> DiscoverCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, ICommand?>(
            nameof(DiscoverCommand));

    public ICommand? DiscoverCommand
    {
        get => GetValue(DiscoverCommandProperty);
        set => SetValue(DiscoverCommandProperty, value);
    }

    // ============================================================
    // Discover Command Parameter
    // ============================================================

    public static readonly StyledProperty<object?> DiscoverCommandParameterProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, object?>(
            nameof(DiscoverCommandParameter));

    public object? DiscoverCommandParameter
    {
        get => GetValue(DiscoverCommandParameterProperty);
        set => SetValue(DiscoverCommandParameterProperty, value);
    }

    // ============================================================
    // View Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ViewCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, ICommand?>(
            nameof(ViewCommand));

    public ICommand? ViewCommand
    {
        get => GetValue(ViewCommandProperty);
        set => SetValue(ViewCommandProperty, value);
    }

    public static readonly StyledProperty<object?> ViewCommandParameterProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationRow, object?>(
            nameof(ViewCommandParameter));

    public object? ViewCommandParameter
    {
        get => GetValue(ViewCommandParameterProperty);
        set => SetValue(ViewCommandParameterProperty, value);
    }
}