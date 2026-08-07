using Avalonia;
using Avalonia.Controls;
using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class DiscoveryProviderRow : UserControl
{
    public DiscoveryProviderRow()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<ProviderInfo?> ProviderProperty =
        AvaloniaProperty.Register<DiscoveryProviderRow, ProviderInfo?>(
            nameof(Provider));

    public ProviderInfo? Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }
}