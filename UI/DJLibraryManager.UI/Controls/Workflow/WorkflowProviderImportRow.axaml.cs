using System.Collections.Generic;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowProviderImportRow : UserControl
{
    public WorkflowProviderImportRow()
    {
        InitializeComponent();
    }

    // ============================================================
    // Provider
    // ============================================================

    public static readonly StyledProperty<ProviderInfo?> ProviderProperty =
        AvaloniaProperty.Register<WorkflowProviderImportRow, ProviderInfo?>(
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
        AvaloniaProperty.Register<WorkflowProviderImportRow, ICommand?>(
            nameof(ImportCommand));

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }

    // ============================================================
    // Import Command Parameter
    // ============================================================

    public static readonly StyledProperty<object?> ImportCommandParameterProperty =
        AvaloniaProperty.Register<WorkflowProviderImportRow, object?>(
            nameof(ImportCommandParameter));

    public object? ImportCommandParameter
    {
        get => GetValue(ImportCommandParameterProperty);
        set => SetValue(ImportCommandParameterProperty, value);
    }
}