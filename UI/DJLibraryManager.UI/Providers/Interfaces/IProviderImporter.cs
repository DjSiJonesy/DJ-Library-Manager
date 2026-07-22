using System.Threading.Tasks;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;

namespace DJLibraryManager.UI.Providers.Interfaces;

public interface IProviderImporter
{
    string ProviderName { get; }

    Task<ImportResult> ImportAsync(ProviderInfo provider);
}