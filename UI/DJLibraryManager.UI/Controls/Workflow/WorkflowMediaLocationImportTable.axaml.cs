﻿using System.Collections.Generic;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models.Import;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaLocationImportTable : UserControl
{
    public WorkflowMediaLocationImportTable()
    {
        InitializeComponent();
    }

    // ============================================================
    // Media Locations
    // ============================================================

    public static readonly StyledProperty<IEnumerable<MediaLocationImportInfo>?> MediaLocationsProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, IEnumerable<MediaLocationImportInfo>?>(
            nameof(MediaLocations));

    public IEnumerable<MediaLocationImportInfo>? MediaLocations
    {
        get => GetValue(MediaLocationsProperty);
        set => SetValue(MediaLocationsProperty, value);
    }

    // ============================================================
    // Location Count
    // ============================================================

    public static readonly StyledProperty<int> LocationCountProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(LocationCount));

    public int LocationCount
    {
        get => GetValue(LocationCountProperty);
        set => SetValue(LocationCountProperty, value);
    }

    // ============================================================
    // Total Tracks
    // ============================================================

    public static readonly StyledProperty<int> TotalTracksProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalTracks));

    public int TotalTracks
    {
        get => GetValue(TotalTracksProperty);
        set => SetValue(TotalTracksProperty, value);
    }

    // ============================================================
    // Total Existing
    // ============================================================

    public static readonly StyledProperty<int> TotalExistingProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalExisting));

    public int TotalExisting
    {
        get => GetValue(TotalExistingProperty);
        set => SetValue(TotalExistingProperty, value);
    }

    // ============================================================
    // Total Imported
    // ============================================================

    public static readonly StyledProperty<int> TotalImportedProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalImported));

    public int TotalImported
    {
        get => GetValue(TotalImportedProperty);
        set => SetValue(TotalImportedProperty, value);
    }

    // ============================================================
    // Total Failed
    // ============================================================

    public static readonly StyledProperty<int> TotalFailedProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, int>(
            nameof(TotalFailed));

    public int TotalFailed
    {
        get => GetValue(TotalFailedProperty);
        set => SetValue(TotalFailedProperty, value);
    }

    // ============================================================
    // Import Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkflowMediaLocationImportTable, ICommand?>(
            nameof(ImportCommand));

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }
}