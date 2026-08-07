using System.Collections.Generic;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowProviderImportTable : UserControl
{
    public WorkflowProviderImportTable()
    {
        InitializeComponent();
    }

    // ============================================================
    // Providers
    // ============================================================

    public static readonly StyledProperty<IEnumerable<ProviderInfo>?> ProvidersProperty =
        AvaloniaProperty.Register<WorkflowProviderImportTable, IEnumerable<ProviderInfo>?>(
            nameof(Providers));

    public IEnumerable<ProviderInfo>? Providers
    {
        get => GetValue(ProvidersProperty);
        set => SetValue(ProvidersProperty, value);
    }

    // ============================================================
    // Provider Count
    // ============================================================

    public static readonly StyledProperty<int> ProviderCountProperty =
        AvaloniaProperty.Register<WorkflowProviderImportTable, int>(
            nameof(ProviderCount));

    public int ProviderCount
    {
        get => GetValue(ProviderCountProperty);
        set => SetValue(ProviderCountProperty, value);
    }

    // ============================================================
    // Total Tracks
    // ============================================================

    public static readonly StyledProperty<int> TotalTracksProperty =
        AvaloniaProperty.Register<WorkflowProviderImportTable, int>(
            nameof(TotalTracks));

    public int TotalTracks
    {
        get => GetValue(TotalTracksProperty);
        set => SetValue(TotalTracksProperty, value);
    }

    // ============================================================
    // Total Playlists
    // ============================================================

    public static readonly StyledProperty<int> TotalPlaylistsProperty =
        AvaloniaProperty.Register<WorkflowProviderImportTable, int>(
            nameof(TotalPlaylists));

    public int TotalPlaylists
    {
        get => GetValue(TotalPlaylistsProperty);
        set => SetValue(TotalPlaylistsProperty, value);
    }

    // ============================================================
    // Import Command
    // ============================================================

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkflowProviderImportTable, ICommand?>(
            nameof(ImportCommand));

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }
}