using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using DJLibraryManager.UI.Models.Improve;
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
        EventArgs e)
    {
        if (DataContext is ViewModels.Improve.ImproveWorkspaceViewModel viewModel)
        {
            viewModel.ConfirmationRequested -= ViewModel_ConfirmationRequested;
            viewModel.ConfirmationRequested += ViewModel_ConfirmationRequested;

            viewModel.RemovalInstructionsRequested -= ViewModel_RemovalInstructionsRequested;
            viewModel.RemovalInstructionsRequested += ViewModel_RemovalInstructionsRequested;
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

    private async Task ViewModel_RemovalInstructionsRequested(
        IReadOnlyCollection<ProviderRemovalInstructions> instructions)
    {
        await ShowRemovalInstructionsAsync(instructions);
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

    private async Task ShowRemovalInstructionsAsync(
        IReadOnlyCollection<ProviderRemovalInstructions> instructions)
    {
        var window = TopLevel.GetTopLevel(this) as Window;

        if (window is null)
            return;

        var dialog = new ProviderRemovalInstructionsDialog(instructions);

        await dialog.ShowDialog(window);
    }
}