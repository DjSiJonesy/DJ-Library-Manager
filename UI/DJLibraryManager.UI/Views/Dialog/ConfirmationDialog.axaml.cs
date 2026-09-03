using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DJLibraryManager.UI.Views.Dialogs;

public partial class ConfirmationDialog : Window
{
    public string ConfirmationMessage { get; }
    public string ConfirmButtonText { get; }

    public ConfirmationDialog(
        string title,
        string message,
        string confirmButtonText = "Confirm")
    {
        InitializeComponent();

        Title = title;
        ConfirmationMessage = message;
        ConfirmButtonText = confirmButtonText;

        DataContext = this;
    }

    private void Confirm_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close(true);
    }

    private void Cancel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close(false);
    }
}