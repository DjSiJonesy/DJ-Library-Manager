using DJLibraryManager.UI.Models;
using System;

namespace DJLibraryManager.Core.Services;

/// <summary>
/// Represents the current application state.
///
/// This acts as the central event hub for DIASISS DJ, allowing
/// repositories and services to notify interested ViewModels when
/// application data changes.
///
/// ApplicationState does not store application data.
/// It simply broadcasts notifications that something has changed.
/// </summary>
public sealed class ApplicationState
{
    // ============================================================
    // Provider Discovery
    // ============================================================

    public event EventHandler? ProvidersChanged;

    public void NotifyProvidersChanged()
    {
        ProvidersChanged?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Media Locations
    // ============================================================

    public event EventHandler? MediaLocationsChanged;

    public void NotifyMediaLocationsChanged()
    {
        MediaLocationsChanged?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Media Discovery
    // ============================================================

    public event EventHandler? DiscoveryChanged;

    public void NotifyDiscoveryChanged()
    {
        DiscoveryChanged?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Library Import
    // ============================================================

    public event EventHandler? LibraryImported;

    public void NotifyLibraryImported()
    {
        LibraryImported?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Analysis
    // ============================================================

    public event EventHandler? AnalysisCompleted;

    public void NotifyAnalysisCompleted()
    {
        AnalysisCompleted?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Recovery
    // ============================================================

    public event EventHandler? RecoveryCompleted;

    public void NotifyRecoveryCompleted()
    {
        RecoveryCompleted?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Settings
    // ============================================================

    public event EventHandler? SettingsChanged;

    public void NotifySettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    // ============================================================
    // Workspace Navigation
    // ============================================================

    public event EventHandler<WorkspaceType>? NavigateRequested;

    public void NavigateTo(WorkspaceType workspace)
    {
        NavigateRequested?.Invoke(this, workspace);
    }
}