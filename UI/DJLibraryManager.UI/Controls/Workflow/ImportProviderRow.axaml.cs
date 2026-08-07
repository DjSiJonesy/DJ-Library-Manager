using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowProviderRow : UserControl
{
    public WorkflowProviderRow()
    {
        InitializeComponent();
    }

    // ============================================================
    // Provider
    // ============================================================

    public static readonly StyledProperty<ProviderInfo?> ProviderProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, ProviderInfo?>(
            nameof(Provider));

    public ProviderInfo? Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    // ============================================================
    // Import Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkflowProviderRow, ICommand?>(
            nameof(ImportCommand));

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }
}