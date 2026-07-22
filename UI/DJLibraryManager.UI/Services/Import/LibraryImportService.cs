using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Services.Import;

public sealed class LibraryImportService
{
    private readonly List<IProviderImporter> _importers = new();
    private readonly IProgressReporter _progressReporter;

    public LibraryImportService(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter
            ?? throw new ArgumentNullException(nameof(progressReporter));
    }

    public void Register(IProviderImporter importer)
    {
        ArgumentNullException.ThrowIfNull(importer);

        _importers.Add(importer);
    }

    public async Task<ImportResult> ImportAsync(ProviderInfo provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        _progressReporter.BeginOperation($"Import {provider.Name} Library");

        try
        {
            _progressReporter.ReportStage("Locating importer...");

            var importer = _importers.FirstOrDefault(i =>
                i.ProviderName.Equals(provider.Name, StringComparison.OrdinalIgnoreCase));

            if (importer is null)
            {
                const string message = "No importer has been registered.";

                _progressReporter.Fail(message);

                return new ImportResult
                {
                    Success = false,
                    ProviderName = provider.Name,
                    ErrorMessage = $"{message} ({provider.Name})"
                };
            }

            _progressReporter.ReportStage("Importing library...");

            var result = await importer.ImportAsync(provider);

            if (result.Success)
            {
                _progressReporter.ReportStage("Finalising...");

                _progressReporter.Complete();
            }
            else
            {
                _progressReporter.Fail(result.ErrorMessage ?? "Import failed.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _progressReporter.Fail(ex.Message);
            throw;
        }
    }
}