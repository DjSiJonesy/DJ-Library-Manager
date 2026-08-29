using DJLibraryManager.Core.Models.Library;
using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.Services.Import;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.Core.Services.Library;

/// <summary>
/// Provides consolidated statistics for the DIASISS Library.
///
/// All UI components should retrieve library statistics through
/// this service rather than calculating them independently.
///
/// Each statistic is obtained from the authoritative subsystem
/// responsible for that information:
///
///     DiscoveryRepository
///         Drives
///         Folders
///         Audio files
///         Video files
///         Total size
///
///     LibraryRepository
///         DIASISS library tracks
///         DIASISS playlists
///         Provider statistics
///
///     MediaImportRepository
///         Media import statistics
/// </summary>
public sealed class LibraryStatisticsService
{
    private readonly LibraryRepository _libraryRepository;
    private readonly MediaImportRepository _mediaImportRepository;
    private readonly DiscoveryRepository _discoveryRepository;

    public LibraryStatisticsService(
        LibraryRepository libraryRepository,
        MediaImportRepository mediaImportRepository,
        DiscoveryRepository discoveryRepository)
    {
        _libraryRepository =
            libraryRepository
            ?? throw new ArgumentNullException(
                nameof(libraryRepository));

        _mediaImportRepository =
            mediaImportRepository
            ?? throw new ArgumentNullException(
                nameof(mediaImportRepository));

        _discoveryRepository =
            discoveryRepository
            ?? throw new ArgumentNullException(
                nameof(discoveryRepository));
    }

    // ============================================================
    // Statistics
    // ============================================================

    /// <summary>
    /// Returns the current consolidated DIASISS library statistics.
    ///
    /// Discovery statistics use the same definitions already used
    /// by the Discovery workspace.
    ///
    /// DIASISS library statistics use the authoritative SQLite
    /// library.
    ///
    /// Media import statistics use the media import repository.
    /// </summary>
    public async Task<LibraryStatistics> GetStatisticsAsync()
    {
        // --------------------------------------------------------
        // Discovery statistics
        //
        // These deliberately come from DiscoveryRepository so the
        // Library Overview uses exactly the same definitions as
        // the Discovery workspace.
        // --------------------------------------------------------

        var discoverySessions =
            _discoveryRepository
                .DiscoverySessions;

        var driveCount =
            discoverySessions.Count;

        var folderCount =
            discoverySessions.Sum(
                x => x.Libraries.Count);

        var audioFileCount =
            discoverySessions.Sum(
                x => x.Libraries.Sum(
                    library => library.AudioFileCount));

        var videoFileCount =
            discoverySessions.Sum(
                x => x.Libraries.Sum(
                    library => library.VideoFileCount));

        var totalSizeBytes =
            discoverySessions.Sum(
                x => x.Libraries.Sum(
                    library => library.TotalSizeBytes));

        // --------------------------------------------------------
        // Media import statistics
        // --------------------------------------------------------

        var importRecords =
            _mediaImportRepository.GetAll();

        // --------------------------------------------------------
        // DIASISS library statistics
        // --------------------------------------------------------

        var libraryTrackCount =
            await _libraryRepository
                .GetTrackCountAsync();

        var libraryPlaylistCount =
            await _libraryRepository
                .GetPlaylistCountAsync();

        // --------------------------------------------------------
        // Provider statistics
        //
        // These currently use the DIASISS library totals, matching
        // the existing architecture.
        // --------------------------------------------------------

        var providerTrackCount =
            await _libraryRepository
                .GetTrackCountAsync();

        var providerPlaylistCount =
            await _libraryRepository
                .GetPlaylistCountAsync();

        // --------------------------------------------------------
        // Return consolidated statistics
        // --------------------------------------------------------

        return new LibraryStatistics
        {
            // =====================================================
            // DIASISS Library
            // =====================================================

            LibraryTrackCount =
                libraryTrackCount,

            LibraryPlaylistCount =
                libraryPlaylistCount,

            // =====================================================
            // Discovery / Library Overview
            // =====================================================

            DriveCount =
                driveCount,

            FolderCount =
                folderCount,

            AudioFileCount =
                audioFileCount,

            VideoFileCount =
                videoFileCount,

            TotalSizeBytes =
                totalSizeBytes,

            // =====================================================
            // Provider Imports
            // =====================================================

            ProviderTrackCount =
                providerTrackCount,

            ProviderPlaylistCount =
                providerPlaylistCount,

            // =====================================================
            // Media Imports
            // =====================================================

            DiscoveredTrackCount =
                importRecords.Sum(
                    x => x.TotalFiles),

            ImportedMediaTrackCount =
                importRecords.Sum(
                    x => x.ImportedFiles),

            ExistingMediaTrackCount =
                importRecords.Sum(
                    x => x.SkippedFiles),

            FailedMediaImports =
                importRecords.Sum(
                    x => x.FailedFiles)
        };
    }
}