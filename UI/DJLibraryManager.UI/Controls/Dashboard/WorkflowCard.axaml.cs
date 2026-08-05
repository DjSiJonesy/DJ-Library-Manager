using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DJLibraryManager.Core.Workflow;
using System;
using System.Windows.Input;

namespace DJLibraryManager.UI.Controls.Dashboard;

public partial class WorkflowCard : UserControl
{
    private readonly IBrush? _normalBorderBrush;
    private readonly IBrush? _hoverBorderBrush;

    public WorkflowCard()
    {
        InitializeComponent();

        _normalBorderBrush =
            Application.Current?.FindResource("Brush.Panel.Border") as IBrush;

        _hoverBorderBrush =
            Application.Current?.FindResource("Brush.Accent") as IBrush;
    }

    private void Button_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (_hoverBorderBrush is not null)
        {
            CardBorder.BorderBrush = _hoverBorderBrush;
        }

        HoverAction?.Invoke(Stage);
    }

    private void Button_PointerExited(object? sender, PointerEventArgs e)
    {
        if (_normalBorderBrush is not null)
        {
            CardBorder.BorderBrush = _normalBorderBrush;
        }

        HoverAction?.Invoke(WorkflowStage.Welcome);
    }

    // ------------------------------------------------------------
    // Icon
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(Icon),
            string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ------------------------------------------------------------
    // Title
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(Title),
            string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ------------------------------------------------------------
    // Description
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(Description),
            string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ------------------------------------------------------------
    // Status
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(Status),
            string.Empty);

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly StyledProperty<IBrush> StatusBrushProperty =
        AvaloniaProperty.Register<WorkflowCard, IBrush>(
            nameof(StatusBrush),
            Brushes.Gray);

    public IBrush StatusBrush
    {
        get => GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    // ------------------------------------------------------------
    // Primary Statistic
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> PrimaryStatisticTitleProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(PrimaryStatisticTitle),
            string.Empty);

    public string PrimaryStatisticTitle
    {
        get => GetValue(PrimaryStatisticTitleProperty);
        set => SetValue(PrimaryStatisticTitleProperty, value);
    }

    public static readonly StyledProperty<string> PrimaryStatisticValueProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(PrimaryStatisticValue),
            string.Empty);

    public string PrimaryStatisticValue
    {
        get => GetValue(PrimaryStatisticValueProperty);
        set => SetValue(PrimaryStatisticValueProperty, value);
    }

    // ------------------------------------------------------------
    // Secondary Statistic
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> SecondaryStatisticTitleProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(SecondaryStatisticTitle),
            string.Empty);

    public string SecondaryStatisticTitle
    {
        get => GetValue(SecondaryStatisticTitleProperty);
        set => SetValue(SecondaryStatisticTitleProperty, value);
    }

    public static readonly StyledProperty<string> SecondaryStatisticValueProperty =
        AvaloniaProperty.Register<WorkflowCard, string>(
            nameof(SecondaryStatisticValue),
            string.Empty);

    public string SecondaryStatisticValue
    {
        get => GetValue(SecondaryStatisticValueProperty);
        set => SetValue(SecondaryStatisticValueProperty, value);
    }

    // ------------------------------------------------------------
    // Action
    // ------------------------------------------------------------

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<WorkflowCard, ICommand?>(
            nameof(ActionCommand));

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly StyledProperty<WorkflowStage> StageProperty =
        AvaloniaProperty.Register<WorkflowCard, WorkflowStage>(
            nameof(Stage));

    public WorkflowStage Stage
    {
        get => GetValue(StageProperty);
        set => SetValue(StageProperty, value);
    }

    public static readonly StyledProperty<Action<WorkflowStage>?> HoverActionProperty =
        AvaloniaProperty.Register<WorkflowCard, Action<WorkflowStage>?>(
            nameof(HoverAction));

    public Action<WorkflowStage>? HoverAction
    {
        get => GetValue(HoverActionProperty);
        set => SetValue(HoverActionProperty, value);
    }
}