using System.Windows.Input;

using Avalonia.Controls;

using DJLibraryManager.UI.ViewModels;

namespace DJLibraryManager.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is DashboardViewModel vm)
            {
                SelectProviderCommand = vm.SelectProviderCommand;
            }
        };
    }

    public ICommand? SelectProviderCommand { get; private set; }
}