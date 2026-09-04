using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Interactivity;

using DJLibraryManager.UI.Models.Improve;

namespace DJLibraryManager.UI.Views.Dialogs;

public partial class ProviderRemovalInstructionsDialog : Window
{
    public IReadOnlyCollection<ProviderRemovalInstructions> Instructions { get; }

    public string DialogTitle { get; }

    public string DialogDescription { get; }

    public ProviderRemovalInstructionsDialog(
        IReadOnlyCollection<ProviderRemovalInstructions> instructions)
    {
        InitializeComponent();

        Instructions = instructions;

        DialogTitle = "Provider Removal Instructions";

        DialogDescription =
            "Use the originating DJ application to remove missing-file " +
            "records. DIASISS does not modify provider databases.";

        DataContext = this;
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }
}