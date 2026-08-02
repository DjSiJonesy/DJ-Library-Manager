using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;

using DJLibraryManager.UI.Models.Explorer;

namespace DJLibraryManager.UI.Controls.Explorer.Tree;

public partial class ExplorerTree : UserControl
{
    public ExplorerTree()
    {
        InitializeComponent();
    }

    // ------------------------------------------------------------
    // Nodes
    // ------------------------------------------------------------

    public static readonly StyledProperty<ObservableCollection<ExplorerNode>> NodesProperty =
        AvaloniaProperty.Register<ExplorerTree, ObservableCollection<ExplorerNode>>(
            nameof(Nodes),
            new ObservableCollection<ExplorerNode>());

    public ObservableCollection<ExplorerNode> Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }
}