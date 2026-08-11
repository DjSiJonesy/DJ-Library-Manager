using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowMediaFolderTable : UserControl
{
    public WorkflowMediaFolderTable()
    {
        InitializeComponent();
    }

    // ============================================================
    // Libraries
    // ============================================================

    public static readonly StyledProperty<IEnumerable<MediaLibrary>?> LibrariesProperty =
        AvaloniaProperty.Register<WorkflowMediaFolderTable, IEnumerable<MediaLibrary>?>(
            nameof(Libraries));

    public IEnumerable<MediaLibrary>? Libraries
    {
        get => GetValue(LibrariesProperty);
        set => SetValue(LibrariesProperty, value);
    }
}