using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace DJLibraryManager.UI.ViewModels.Providers;

/// <summary>
/// View model representing a provider card on the dashboard.
/// </summary>
public partial class ProviderCardViewModel : ObservableObject
{
    public string Name { get; }

    public bool Installed { get; }

    public string Status =>
        Installed ? "✓ Installed" : "Not Installed";

    public Bitmap? ProviderLogo { get; }

    public IRelayCommand OpenCommand { get; }

    public ProviderCardViewModel(
        string name,
        bool installed,
        Bitmap? providerLogo,
        Action<ProviderCardViewModel> openAction)
    {
        Name = name;
        Installed = installed;
        ProviderLogo = providerLogo;

        OpenCommand = new RelayCommand(
            () =>
            {
                if (Installed)
                    openAction(this);
            });
    }
}