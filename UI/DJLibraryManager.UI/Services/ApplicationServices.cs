using DJLibraryManager.UI.Providers.VirtualDJ.Services;
using DJLibraryManager.UI.Services.Import;

namespace DJLibraryManager.UI.Services;

public sealed class ApplicationServices
{
    /// <summary>
    /// Reports progress for long-running operations.
    /// </summary>
    public ProgressReporter ProgressReporter { get; }

    /// <summary>
    /// Stores and retrieves the application's media library.
    /// </summary>
    public LibraryRepository LibraryRepository { get; }

    /// <summary>
    /// Coordinates provider library imports.
    /// </summary>
    public LibraryImportService LibraryImportService { get; }

    public ApplicationServices()
    {
        ProgressReporter = new ProgressReporter();

        LibraryRepository = new LibraryRepository();

        LibraryImportService = new LibraryImportService(
            ProgressReporter,
            LibraryRepository);

        LibraryImportService.Register(
            new VirtualDJImporter(ProgressReporter));
    }
}