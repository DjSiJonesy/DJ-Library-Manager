using DJLibraryManager.Core.Services;

using DJLibraryManager.UI.Providers.VirtualDJ.Services;
using DJLibraryManager.UI.Services.Import;
using DJLibraryManager.Core.Services.Library;
using DJLibraryManager.UI.Services.Discovery;

namespace DJLibraryManager.UI.Services;

public sealed class ApplicationServices
{
    /// <summary>
    /// Reports progress for long-running operations.
    /// </summary>
    public ProgressReporter ProgressReporter { get; }

    /// <summary>
    /// Represents the current application state and
    /// broadcasts application-wide change notifications.
    /// </summary>
    public ApplicationState ApplicationState { get; }

    /// <summary>
    /// Stores and retrieves the application's media library.
    /// </summary>
    public LibraryRepository LibraryRepository { get; }

    /// <summary>
    /// Provides consolidated statistics for the DIASISS Library.
    /// </summary>
    public LibraryStatisticsService LibraryStatisticsService { get; }

    /// <summary>
    /// Stores the application's known media locations.
    /// </summary>
    public MediaLocationRepository MediaLocationRepository { get; }

    /// <summary>
    /// Stores the current media discovery session.
    /// </summary>
    public DiscoveryRepository DiscoveryRepository { get; }

    /// <summary>
    /// Stores Discovery validation history.
    /// </summary>
    public DiscoveryValidationRepository DiscoveryValidationRepository { get; }

    /// <summary>
    /// Coordinates Discovery validation operations.
    /// </summary>
    public DiscoveryValidationWorkflowService DiscoveryValidationWorkflowService { get; }

    /// <summary>
    /// Stores Media Location import history.
    /// </summary>
    public MediaImportRepository MediaImportRepository { get; }

    /// <summary>
    /// Coordinates provider library imports.
    /// </summary>
    public LibraryImportService LibraryImportService { get; }

    /// <summary>
    /// Analyses Media from the Library.
    /// </summary>
    public AnalysisService Analysis { get; }

    public ApplicationServices()
    {
        ProgressReporter = new ProgressReporter();

        ApplicationState = new ApplicationState();

        LibraryRepository = new LibraryRepository();

        MediaLocationRepository = new MediaLocationRepository();

        DiscoveryRepository = new DiscoveryRepository(ApplicationState);

        DiscoveryValidationRepository = new DiscoveryValidationRepository();

        DiscoveryValidationWorkflowService = new DiscoveryValidationWorkflowService(DiscoveryValidationRepository);

        MediaImportRepository = new MediaImportRepository();

        LibraryStatisticsService = new LibraryStatisticsService(LibraryRepository, MediaImportRepository);

        LibraryImportService = new LibraryImportService(ProgressReporter, LibraryRepository);

        LibraryImportService.Register(new VirtualDJImporter(ProgressReporter));

        Analysis = new AnalysisService();
    }
}