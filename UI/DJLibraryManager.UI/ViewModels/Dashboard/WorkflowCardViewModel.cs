using System;
using System.Windows.Input;

using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using DJLibraryManager.Core.Workflow;

namespace DJLibraryManager.UI.ViewModels.Dashboard;

public partial class WorkflowCardViewModel : ObservableObject
{
    // ------------------------------------------------------------
    // Workflow Definition
    // ------------------------------------------------------------

    public required WorkflowDefinition Definition { get; init; }

    public WorkflowStage Stage => Definition.Stage;

    public string Icon => Definition.Icon;

    public string Title => Definition.Name;

    public string Description => Definition.Description;

    // ------------------------------------------------------------
    // Status
    // ------------------------------------------------------------

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    private IBrush statusBrush = Brushes.Gray;

    // ------------------------------------------------------------
    // Primary Statistic
    // ------------------------------------------------------------

    [ObservableProperty]
    private string primaryStatisticTitle = string.Empty;

    [ObservableProperty]
    private string primaryStatisticValue = string.Empty;

    // ------------------------------------------------------------
    // Secondary Statistic
    // ------------------------------------------------------------

    [ObservableProperty]
    private string secondaryStatisticTitle = string.Empty;

    [ObservableProperty]
    private string secondaryStatisticValue = string.Empty;

    // ------------------------------------------------------------
    // Action
    // ------------------------------------------------------------

    public Action<WorkflowStage>? HoverAction { get; init; }

    public ICommand? ActionCommand { get; init; }
}