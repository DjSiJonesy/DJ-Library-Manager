using DJLibraryManager.Core.Services;

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
    /// Stores the application's known media locations.
    /// </summary>
    public MediaLocationRepository MediaLocationRepository { get; } = new();

    /// <summary>
    /// Stores the current media discovery session.
    /// </summary>
    public DiscoveryRepository DiscoveryRepository { get; }

    /// <summary>
    /// Coordinates provider library imports.
    /// </summary>
    public LibraryImportService LibraryImportService { get; }

    public ApplicationServices()
    {
        ProgressReporter = new ProgressReporter();

        LibraryRepository = new LibraryRepository();

        MediaLocationRepository = new MediaLocationRepository();

        DiscoveryRepository = new DiscoveryRepository();

        LibraryImportService = new LibraryImportService(
            ProgressReporter,
            LibraryRepository);

        LibraryImportService.Register(
            new VirtualDJImporter(ProgressReporter));
    }
}