namespace DJLibraryManager.UI.ViewModels.Workspace;

/// <summary>
/// Base class for all workspaces hosted within the application's
/// permanent workspace area.
///
/// Every feature displayed on the right-hand side of the shell
/// derives from this class.
/// </summary>
public abstract class WorkspaceViewModel : ViewModelBase
{
    /// <summary>
    /// Display title shown by the workspace.
    /// </summary>
    public virtual string Title => string.Empty;

    /// <summary>
    /// Optional subtitle.
    /// </summary>
    public virtual string Subtitle => string.Empty;

    /// <summary>
    /// Indicates whether the workspace has unsaved changes.
    /// </summary>
    public virtual bool IsDirty => false;
}