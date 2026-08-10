using DJLibraryManager.Core.Models.Library;
using DJLibraryManager.UI.Services;
using DJLibraryManager.UI.Services.Import;
using System.Linq;
using System.Threading.Tasks;

namespace DJLibraryManager.Core.Services.Library;

/// <summary>
/// Provides consolidated statistics for the DIASISS Library.
///
/// All UI components should retrieve library statistics through
/// this service rather than calculating them independently.
/// </summary>
public sealed class LibraryStatisticsService
{
    private readonly LibraryRepository _libraryRepository;
    private readonly MediaImportRepository _mediaImportRepository;

    public LibraryStatisticsService(
        LibraryRepository libraryRepository,
        MediaImportRepository mediaImportRepository)
    {
        _libraryRepository = libraryRepository;
        _mediaImportRepository = mediaImportRepository;
    }

    /// <summary>
    /// Returns the current library statistics.
    /// </summary>
    public async Task<LibraryStatistics> GetStatisticsAsync()
    {
        var importRecords =
            _mediaImportRepository.GetAll();

        return new LibraryStatistics
        {
            // =====================================================
            // DIASISS Library
            // =====================================================

            LibraryTrackCount =
                await _libraryRepository.GetTrackCountAsync(),

            LibraryPlaylistCount =
                await _libraryRepository.GetPlaylistCountAsync(),

            // =====================================================
            // Provider Imports
            // =====================================================

            //
            // For now these are the same values.
            // Later they may diverge as providers are
            // synchronised independently.
            //

            ProviderTrackCount =
                await _libraryRepository.GetTrackCountAsync(),

            ProviderPlaylistCount =
                await _libraryRepository.GetPlaylistCountAsync(),

            // =====================================================
            // Media Imports
            // =====================================================

            DiscoveredTrackCount =
                importRecords.Sum(x => x.TotalFiles),

            ImportedMediaTrackCount =
                importRecords.Sum(x => x.ImportedFiles),

            ExistingMediaTrackCount =
                importRecords.Sum(x => x.SkippedFiles),

            FailedMediaImports =
                importRecords.Sum(x => x.FailedFiles)
        };
    }
}