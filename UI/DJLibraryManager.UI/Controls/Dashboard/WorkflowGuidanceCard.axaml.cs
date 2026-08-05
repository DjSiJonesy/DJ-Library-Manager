using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;

namespace DJLibraryManager.UI.Controls.Dashboard;

public partial class WorkflowGuidanceCard : UserControl
{
    public WorkflowGuidanceCard()
    {
        InitializeComponent();
    }

    // ------------------------------------------------------------
    // Is Welcome
    // ------------------------------------------------------------

    public static readonly StyledProperty<bool> IsWelcomeProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, bool>(
            nameof(IsWelcome),
            true);

    public bool IsWelcome
    {
        get => GetValue(IsWelcomeProperty);
        set => SetValue(IsWelcomeProperty, value);
    }

    // ------------------------------------------------------------
    // Title
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, string>(
            nameof(Title),
            "Workflow Guidance");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ------------------------------------------------------------
    // Description
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, string>(
            nameof(Description),
            string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ------------------------------------------------------------
    // Next Step
    // ------------------------------------------------------------

    public static readonly StyledProperty<string> NextStepProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, string>(
            nameof(NextStep),
            string.Empty);

    public string NextStep
    {
        get => GetValue(NextStepProperty);
        set => SetValue(NextStepProperty, value);
    }

    // ------------------------------------------------------------
    // What Happens
    // ------------------------------------------------------------

    public static readonly StyledProperty<ObservableCollection<string>> WhatHappensProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, ObservableCollection<string>>(
            nameof(WhatHappens),
            new());

    public ObservableCollection<string> WhatHappens
    {
        get => GetValue(WhatHappensProperty);
        set => SetValue(WhatHappensProperty, value);
    }

    // ------------------------------------------------------------
    // Good To Know
    // ------------------------------------------------------------

    public static readonly StyledProperty<ObservableCollection<string>> GoodToKnowProperty =
        AvaloniaProperty.Register<WorkflowGuidanceCard, ObservableCollection<string>>(
            nameof(GoodToKnow),
            new());

    public ObservableCollection<string> GoodToKnow
    {
        get => GetValue(GoodToKnowProperty);
        set => SetValue(GoodToKnowProperty, value);
    }
}