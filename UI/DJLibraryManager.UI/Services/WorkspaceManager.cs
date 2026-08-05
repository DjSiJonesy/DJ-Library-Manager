using CommunityToolkit.Mvvm.ComponentModel;
using DJLibraryManager.UI.ViewModels.Workspace;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Maintains the application's currently active workspace.
///
/// The Shell displays whatever workspace is exposed by this service.
/// </summary>
public partial class WorkspaceManager : ObservableObject
{
    [ObservableProperty]
    private WorkspaceViewModel? activeWorkspace;

    /// <summary>
    /// Displays the supplied workspace.
    /// </summary>
    public void Show(WorkspaceViewModel workspace)
    {
        ActiveWorkspace = workspace;
    }

    /// <summary>
    /// Clears the current workspace.
    /// </summary>
    public void Clear()
    {
        ActiveWorkspace = null;
    }
}