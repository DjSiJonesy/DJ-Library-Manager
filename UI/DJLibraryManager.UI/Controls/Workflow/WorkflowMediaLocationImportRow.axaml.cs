﻿using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models.Import;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaLocationImportRow : UserControl
{
    public WorkflowMediaLocationImportRow()
    {
        InitializeComponent();
    }

    // ============================================================
    // Media Location
    // ============================================================

    public static readonly StyledProperty<MediaLocationImportInfo?> MediaLocationProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportRow, MediaLocationImportInfo?>(
            nameof(MediaLocation));

    public MediaLocationImportInfo? MediaLocation
    {
        get => GetValue(MediaLocationProperty);
        set => SetValue(MediaLocationProperty, value);
    }

    // ============================================================
    // Import Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportRow, ICommand?>(
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
        AvaloniaProperty.Register<WorkflowMediaLocationImportRow, object?>(
            nameof(ImportCommandParameter));

    public object? ImportCommandParameter
    {
        get => GetValue(ImportCommandParameterProperty);
        set => SetValue(ImportCommandParameterProperty, value);
    }
}