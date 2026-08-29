using System;
using System.IO;

using DJLibraryManager.Core.Services;
using DJLibraryManager.Core.Services.Library;

using DJLibraryManager.UI.Data;
using DJLibraryManager.UI.Providers.VirtualDJ.Services;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Services;
using DJLibraryManager.UI.Search.Services.Providers;
using DJLibraryManager.UI.Services.Discovery;
using DJLibraryManager.UI.Services.Import;

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
    /// Provides access to the DIASISS SQLite database.
    ///
    /// SQLite is being introduced alongside the existing JSON
    /// library during the persistence migration.
    /// </summary>
    public SqliteDatabase SqliteDatabase { get; }

    /// <summary>
    /// Stores and retrieves the application's media library.
    /// </summary>
    public LibraryRepository LibraryRepository { get; }

    /// <summary>
    /// Migrates the existing JSON library into SQLite.
    ///
    /// The migration is available to the application but is not
    /// executed automatically during application startup.
    /// </summary>
    public LibraryMigrationService LibraryMigrationService { get; }

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
    public DiscoveryValidationWorkflowService
        DiscoveryValidationWorkflowService
    { get; }

    /// <summary>
    /// Stores Media Location import history.
    /// </summary>
    public MediaImportRepository MediaImportRepository { get; }

    /// <summary>
    /// Coordinates provider library imports.
    /// </summary>
    public LibraryImportService LibraryImportService { get; }

    /// <summary>
    /// Stores the latest Library Analysis.
    /// </summary>
    public AnalysisRepository AnalysisRepository { get; }

    /// <summary>
    /// Analyses Media from the Library.
    /// </summary>
    public AnalysisService Analysis { get; }

    /// <summary>
    /// Stores the latest Search state.
    /// </summary>
    public SearchRepository SearchRepository { get; }

    /// <summary>
    /// Coordinates Search operations.
    /// </summary>
    public SearchService Search { get; }

    public ApplicationServices()
    {
        // ========================================================
        // Core Services
        // ========================================================

        ProgressReporter =
            new ProgressReporter();

        ApplicationState =
            new ApplicationState();

        // ========================================================
        // SQLite
        // ========================================================

        var databasePath =
            Path.Combine(
                ApplicationPaths.Library,
                "DIASISS.db");

        SqliteDatabase =
            new SqliteDatabase(
                databasePath);

        var sqliteSchema =
            new SqliteSchema(
                SqliteDatabase);

        sqliteSchema.EnsureCreated();

        // ========================================================
        // Library
        // ========================================================

        LibraryRepository =
            new LibraryRepository(
                SqliteDatabase);

        LibraryMigrationService =
            new LibraryMigrationService(
                LibraryRepository,
                SqliteDatabase,
                sqliteSchema);

        MediaLocationRepository =
            new MediaLocationRepository();

        // ========================================================
        // Discovery
        // ========================================================

        DiscoveryRepository =
            new DiscoveryRepository(
                ApplicationState);

        DiscoveryValidationRepository =
            new DiscoveryValidationRepository();

        DiscoveryValidationWorkflowService =
            new DiscoveryValidationWorkflowService(
                DiscoveryValidationRepository);

        // ========================================================
        // Import
        // ========================================================

        MediaImportRepository =
            new MediaImportRepository();

        LibraryImportService =
            new LibraryImportService(
                ProgressReporter,
                LibraryRepository);

        LibraryImportService.Register(
            new VirtualDJImporter(
                ProgressReporter));

        // ========================================================
        // Analysis
        // ========================================================

        AnalysisRepository =
            new AnalysisRepository();

        LibraryStatisticsService =
            new LibraryStatisticsService(
                LibraryRepository,
                MediaImportRepository,
                DiscoveryRepository);

        Analysis =
            new AnalysisService(
                LibraryRepository);

        // ========================================================
        // Search
        // ========================================================

        SearchRepository =
            new SearchRepository();

        var duplicateSearchService =
            new DuplicateSearchService(
                LibraryRepository);

        var missingFileSearchService =
            new MissingFileSearchService();

        // --------------------------------------------------------
        // Metadata Search Providers
        // --------------------------------------------------------

        var metadataProviders =
            new IMetadataSearchProvider[]
            {
                new MusicBrainzMetadataProvider(),
                new DiscogsMetadataProvider(),
                new ReccoBeatsMetadataProvider()
            };

        var metadataEnrichmentProviders =
            new IMetadataEnrichmentProvider[]
            {
                new MusicBrainzMetadataEnrichmentProvider(),
                new DiscogsMetadataEnrichmentProvider(),
                new ReccoBeatsMetadataEnrichmentProvider()
            };

        var metadataSearchService =
            new MetadataSearchService(
                metadataProviders,
                enrichmentProviders:
                    metadataEnrichmentProviders);

        // --------------------------------------------------------
        // Other Search Services
        // --------------------------------------------------------

        var musicSearchService =
            new MusicSearchService(
                LibraryRepository);

        var providerSearchService =
            new ProviderSearchService(
                LibraryRepository);

        // --------------------------------------------------------
        // Search Coordinator
        // --------------------------------------------------------

        Search =
            new SearchService(
                duplicateSearchService,
                missingFileSearchService,
                metadataSearchService,
                musicSearchService,
                providerSearchService,
                SearchRepository,
                AnalysisRepository);
    }
}