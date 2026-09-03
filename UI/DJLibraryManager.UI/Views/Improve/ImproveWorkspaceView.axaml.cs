using System.Threading.Tasks;

using Avalonia.Controls;

using DJLibraryManager.UI.Views.Dialogs;

namespace DJLibraryManager.UI.Views.Improve;

public partial class ImproveWorkspaceView : UserControl
{
    public ImproveWorkspaceView()
    {
        InitializeComponent();

        DataContextChanged += ImproveWorkspaceView_DataContextChanged;
    }

    private void ImproveWorkspaceView_DataContextChanged(
        object? sender,
        System.EventArgs e)
    {
        if (DataContext is ViewModels.Improve.ImproveWorkspaceViewModel viewModel)
        {
            viewModel.ConfirmationRequested -= ViewModel_ConfirmationRequested;
            viewModel.ConfirmationRequested += ViewModel_ConfirmationRequested;
        }
    }

    private async Task<bool> ViewModel_ConfirmationRequested(
        string title,
        string message,
        string confirmButtonText)
    {
        return await ShowConfirmationAsync(
            title,
            message,
            confirmButtonText);
    }

    public async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string confirmButtonText = "Confirm")
    {
        var dialog = new ConfirmationDialog(
            title,
            message,
            confirmButtonText);

        var window = TopLevel.GetTopLevel(this) as Window;

        if (window is null)
            return false;

        return await dialog.ShowDialog<bool>(window);
    }
}