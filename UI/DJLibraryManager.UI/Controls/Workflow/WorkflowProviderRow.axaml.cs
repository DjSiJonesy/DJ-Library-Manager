using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowProviderRow : UserControl
{
    public WorkflowProviderRow()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<Bitmap?> ProviderLogoProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, Bitmap?>(nameof(ProviderLogo));

    public Bitmap? ProviderLogo
    {
        get => GetValue(ProviderLogoProperty);
        set => SetValue(ProviderLogoProperty, value);
    }

    public static readonly StyledProperty<string> ProviderNameProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(ProviderName), "");

    public string ProviderName
    {
        get => GetValue(ProviderNameProperty);
        set => SetValue(ProviderNameProperty, value);
    }

    public static readonly StyledProperty<string> InstallationStatusProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(InstallationStatus), "");

    public string InstallationStatus
    {
        get => GetValue(InstallationStatusProperty);
        set => SetValue(InstallationStatusProperty, value);
    }

    public static readonly StyledProperty<IBrush?> InstallationBrushProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, IBrush?>(nameof(InstallationBrush));

    public IBrush? InstallationBrush
    {
        get => GetValue(InstallationBrushProperty);
        set => SetValue(InstallationBrushProperty, value);
    }

    public static readonly StyledProperty<string> LibraryStatusProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(LibraryStatus), "");

    public string LibraryStatus
    {
        get => GetValue(LibraryStatusProperty);
        set => SetValue(LibraryStatusProperty, value);
    }

    public static readonly StyledProperty<string> LastImportedProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(LastImported), "");

    public string LastImported
    {
        get => GetValue(LastImportedProperty);
        set => SetValue(LastImportedProperty, value);
    }

    public static readonly StyledProperty<string> TrackCountProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(TrackCount), "");

    public string TrackCount
    {
        get => GetValue(TrackCountProperty);
        set => SetValue(TrackCountProperty, value);
    }

    public static readonly StyledProperty<string> PlaylistCountProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, string>(nameof(PlaylistCount), "");

    public string PlaylistCount
    {
        get => GetValue(PlaylistCountProperty);
        set => SetValue(PlaylistCountProperty, value);
    }
}