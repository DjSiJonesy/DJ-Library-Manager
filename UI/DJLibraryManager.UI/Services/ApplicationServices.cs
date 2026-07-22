using DJLibraryManager.UI.Providers.VirtualDJ.Services;
using DJLibraryManager.UI.Services.Import;

namespace DJLibraryManager.UI.Services;

public sealed class ApplicationServices
{
    public ProgressReporter ProgressReporter { get; }

    public LibraryImportService LibraryImportService { get; }

    public ApplicationServices()
    {
        ProgressReporter = new ProgressReporter();

        LibraryImportService = new LibraryImportService(ProgressReporter);

        LibraryImportService.Register(
            new VirtualDJImporter(ProgressReporter));
    }
}