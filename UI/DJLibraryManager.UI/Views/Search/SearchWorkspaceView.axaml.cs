using Avalonia.Controls;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.ViewModels.Search;

namespace DJLibraryManager.UI.Views.Search;

/// <summary>
/// View for the Search workspace.
/// </summary>
public partial class SearchWorkspaceView : UserControl
{
    public SearchWorkspaceView()
    {
        InitializeComponent();
    }

    private void WorkflowSearchResults_ResultSelected(
        object? sender,
        SearchResult result)
    {
        if (DataContext is not SearchWorkspaceViewModel viewModel)
            return;

        viewModel.SelectResult(result);
    }
}