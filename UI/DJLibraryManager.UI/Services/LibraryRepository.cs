using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DJLibraryManager.Core.Services;

using DJLibraryManager.UI.Data;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Library;
using DJLibraryManager.UI.Models.Media;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Stores and retrieves the application's provider-independent media library.
///
/// SQLite is the authoritative persistence store for:
///
///     Media
///     Providers
///     MediaProviderIdentities
///     ProviderImports
///
/// Provider-specific relationships are stored through:
///
///     Providers
///     Media
///     MediaProviderIdentities
///
/// Provider import metadata is stored through:
///
///     ProviderImports
/// </summary>
public sealed class LibraryRepository
{
    private readonly SqliteDatabase _database;

    public LibraryRepository(
        SqliteDatabase database)
    {
        ArgumentNullException.ThrowIfNull(
            database);

        _database =
            database;
    }

    // ============================================================
    // Library Existence
    // ============================================================

    /// <summary>
    /// Returns true if the SQLite media library contains records.
    /// </summary>
    public bool Exists()
    {
        using var connection =
            _database.OpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT EXISTS
            (
                SELECT 1
                FROM Media
                LIMIT 1
            );
            """;

        return Convert.ToInt32(
                   command.ExecuteScalar()) == 1;
    }

    // ============================================================
    // Provider Library
    // ============================================================

    /// <summary>
    /// Returns true if the specified provider has media
    /// associated with the DIASISS library.
    /// </summary>
    public async Task<bool> ProviderLibraryExistsAsync(
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM MediaProviderIdentities mpi
                    INNER JOIN Providers p
                        ON p.ProviderId = mpi.ProviderId
                    WHERE p.Name = $providerName
                );
                """;

            command.Parameters.AddWithValue(
                "$providerName",
                providerName);

            return Convert.ToInt32(
                       command.ExecuteScalar()) == 1;
        });
    }

    /// <summary>
    /// Returns the number of media records associated with
    /// the specified provider.
    /// </summary>
    public async Task<int> GetProviderTrackCountAsync(
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT COUNT(DISTINCT mpi.MediaId)
                FROM MediaProviderIdentities mpi
                INNER JOIN Providers p
                    ON p.ProviderId = mpi.ProviderId
                WHERE p.Name = $providerName;
                """;

            command.Parameters.AddWithValue(
                "$providerName",
                providerName);

            return Convert.ToInt32(
                command.ExecuteScalar());
        });
    }

    // ============================================================
    // Save Provider Library
    // ============================================================

    /// <summary>
    /// Saves the imported library for a single provider.
    ///
    /// Existing provider identities are removed first.
    ///
    /// Media records belonging to other providers are retained.
    ///
    /// If an incoming file path already exists in Media, the existing
    /// Media record is reused rather than creating another physical
    /// track record.
    /// </summary>
    public async Task SaveProviderLibraryAsync(
        string providerName,
        IEnumerable<DJLMMediaItem> mediaItems)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        ArgumentNullException.ThrowIfNull(
            mediaItems);

        var items =
            mediaItems
                .Where(x =>
                    x is not null &&
                    !string.IsNullOrWhiteSpace(x.FilePath))
                .ToList();

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                var providerId =
                    GetOrCreateProvider(
                        connection,
                        transaction,
                        providerName);

                //
                // Remove the provider's existing relationships.
                //
                RemoveProviderIdentities(
                    connection,
                    transaction,
                    providerId);

                //
                // Remove Media records which no longer belong
                // to any provider.
                //
                RemoveOrphanedMedia(
                    connection,
                    transaction);

                //
                // Add the current provider library.
                //
                foreach (var mediaItem in items)
                {
                    var mediaId =
                        GetOrCreateMedia(
                            connection,
                            transaction,
                            mediaItem);

                    AddProviderIdentity(
                        connection,
                        transaction,
                        mediaId,
                        providerId);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    // ============================================================
    // Load
    // ============================================================

    /// <summary>
    /// Loads the complete provider-independent media library.
    ///
    /// Because DJLMMediaItem currently exposes a single Provider
    /// property, the first provider identity associated with a Media
    /// record is returned as the representative provider.
    ///
    /// The underlying SQLite model still permits multiple providers.
    /// </summary>
    public async Task<IReadOnlyList<DJLMMediaItem>> LoadAsync()
    {
        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    m.MediaId,
                    m.TrackStatusId,
                    m.MediaType,
                    m.FilePath,
                    m.FileSize,
                    m.Artist,
                    m.Title,
                    m.Album,
                    m.Genre,
                    m.Year,
                    m.BPM,
                    m.MusicalKey,
                    m.DurationSeconds,
                    m.DateFirstSeen,
                    m.DateLastModified,
                    m.CreatedDate,
                    m.LastModifiedDate,

                    (
                        SELECT p.Name
                        FROM MediaProviderIdentities mpi
                        INNER JOIN Providers p
                            ON p.ProviderId = mpi.ProviderId
                        WHERE mpi.MediaId = m.MediaId
                        ORDER BY mpi.MediaProviderIdentityId
                        LIMIT 1
                    ) AS ProviderName

                FROM Media m
                ORDER BY m.MediaId;
                """;

            using var reader =
                command.ExecuteReader();

            var results =
                new List<DJLMMediaItem>();

            while (reader.Read())
            {
                results.Add(
                    ReadMediaItem(reader));
            }

            return (IReadOnlyList<DJLMMediaItem>)results;
        });
    }

    // ============================================================
    // Media Existence
    // ============================================================

    /// <summary>
    /// Returns true if a media record exists for the supplied
    /// file path.
    /// </summary>
    public async Task<bool> MediaExistsAsync(
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM Media
                    WHERE FilePath = $filePath
                );
                """;

            command.Parameters.AddWithValue(
                "$filePath",
                filePath);

            return Convert.ToInt32(
                       command.ExecuteScalar()) == 1;
        });
    }

    // ============================================================
    // Get Media
    // ============================================================

    /// <summary>
    /// Returns the media item for the specified file path.
    /// </summary>
    public async Task<DJLMMediaItem?> GetMediaItemAsync(
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    m.MediaId,
                    m.TrackStatusId,
                    m.MediaType,
                    m.FilePath,
                    m.FileSize,
                    m.Artist,
                    m.Title,
                    m.Album,
                    m.Genre,
                    m.Year,
                    m.BPM,
                    m.MusicalKey,
                    m.DurationSeconds,
                    m.DateFirstSeen,
                    m.DateLastModified,
                    m.CreatedDate,
                    m.LastModifiedDate,

                    (
                        SELECT p.Name
                        FROM MediaProviderIdentities mpi
                        INNER JOIN Providers p
                            ON p.ProviderId = mpi.ProviderId
                        WHERE mpi.MediaId = m.MediaId
                        ORDER BY mpi.MediaProviderIdentityId
                        LIMIT 1
                    ) AS ProviderName

                FROM Media m
                WHERE m.FilePath = $filePath
                LIMIT 1;
                """;

            command.Parameters.AddWithValue(
                "$filePath",
                filePath);

            using var reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            return ReadMediaItem(reader);
        });
    }


    // ============================================================
    // Get Providers For Media
    // ============================================================

    /// <summary>
    /// Returns every provider associated with the specified DIASISS
    /// MediaId.
    ///
    /// MediaId is the authoritative DIASISS media identity. Provider
    /// relationships are resolved through MediaProviderIdentities and
    /// Providers.
    ///
    /// This method is read-only and does not modify the library.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetProviderNamesForMediaAsync(
        string mediaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            mediaId);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
            SELECT DISTINCT
                p.Name
            FROM MediaProviderIdentities mpi
            INNER JOIN Providers p
                ON p.ProviderId = mpi.ProviderId
            WHERE mpi.MediaId = $mediaId
            ORDER BY p.Name;
            """;

            command.Parameters.AddWithValue(
                "$mediaId",
                mediaId);

            using var reader =
                command.ExecuteReader();

            var providers =
                new List<string>();

            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    var providerName =
                        reader.GetString(0);

                    if (!string.IsNullOrWhiteSpace(providerName))
                    {
                        providers.Add(providerName);
                    }
                }
            }

            return (IReadOnlyList<string>)providers;
        });
    }

    // ============================================================
    // Add Media
    // ============================================================

    /// <summary>
    /// Adds a single media item to the SQLite library.
    ///
    /// If the file already exists, the existing Media record is
    /// reused and the provider relationship is added.
    /// </summary>
    public async Task AddMediaItemAsync(
        DJLMMediaItem mediaItem)
    {
        ArgumentNullException.ThrowIfNull(
            mediaItem);

        if (string.IsNullOrWhiteSpace(
                mediaItem.FilePath))
        {
            throw new ArgumentException(
                "Media item must contain a file path.",
                nameof(mediaItem));
        }

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                var providerId =
                    GetOrCreateProvider(
                        connection,
                        transaction,
                        mediaItem.Provider);

                var mediaId =
                    GetOrCreateMedia(
                        connection,
                        transaction,
                        mediaItem);

                AddProviderIdentity(
                    connection,
                    transaction,
                    mediaId,
                    providerId);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    // ============================================================
    // Load Provider Library
    // ============================================================

    /// <summary>
    /// Returns all media associated with a specific provider.
    /// </summary>
    public async Task<IReadOnlyList<DJLMMediaItem>>
        LoadProviderLibraryAsync(
            string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    m.MediaId,
                    m.TrackStatusId,
                    m.MediaType,
                    m.FilePath,
                    m.FileSize,
                    m.Artist,
                    m.Title,
                    m.Album,
                    m.Genre,
                    m.Year,
                    m.BPM,
                    m.MusicalKey,
                    m.DurationSeconds,
                    m.DateFirstSeen,
                    m.DateLastModified,
                    m.CreatedDate,
                    m.LastModifiedDate,

                    p.Name AS ProviderName

                FROM Media m

                INNER JOIN MediaProviderIdentities mpi
                    ON mpi.MediaId = m.MediaId

                INNER JOIN Providers p
                    ON p.ProviderId = mpi.ProviderId

                WHERE p.Name = $providerName

                ORDER BY m.FilePath;
                """;

            command.Parameters.AddWithValue(
                "$providerName",
                providerName);

            using var reader =
                command.ExecuteReader();

            var results =
                new List<DJLMMediaItem>();

            while (reader.Read())
            {
                results.Add(
                    ReadMediaItem(reader));
            }

            return (IReadOnlyList<DJLMMediaItem>)results;
        });
    }

    // ============================================================
    // Import Metadata
    // ============================================================

    /// <summary>
    /// Saves provider import metadata into SQLite.
    ///
    /// ProviderImports contains one record per provider.
    /// An existing record is updated; otherwise a new record is
    /// created.
    /// </summary>
    public async Task SaveProviderImportAsync(
        ProviderInfo provider,
        ImportResult result)
    {
        ArgumentNullException.ThrowIfNull(
            provider);

        ArgumentNullException.ThrowIfNull(
            result);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                var providerId =
                    GetOrCreateProvider(
                        connection,
                        transaction,
                        provider.Name);

                using var command =
                    connection.CreateCommand();

                command.Transaction =
                    transaction;

                command.CommandText =
                    """
                    INSERT INTO ProviderImports
                    (
                        ProviderId,
                        LastImported,
                        TrackCount,
                        PlaylistCount
                    )
                    VALUES
                    (
                        $providerId,
                        $lastImported,
                        $trackCount,
                        $playlistCount
                    )
                    ON CONFLICT(ProviderId)
                    DO UPDATE SET
                        LastImported = excluded.LastImported,
                        TrackCount = excluded.TrackCount,
                        PlaylistCount = excluded.PlaylistCount;
                    """;

                command.Parameters.AddWithValue(
                    "$providerId",
                    providerId);

                command.Parameters.AddWithValue(
                    "$lastImported",
                    result.ImportedAt);

                command.Parameters.AddWithValue(
                    "$trackCount",
                    result.TrackCount);

                command.Parameters.AddWithValue(
                    "$playlistCount",
                    result.PlaylistCount);

                command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    /// <summary>
    /// Returns stored provider import metadata.
    /// </summary>
    public async Task<ProviderLibraryMetadata?>
        GetProviderImportAsync(
            string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
            SELECT
                p.Name AS ProviderName,
                pi.LastImported,
                pi.TrackCount,
                pi.PlaylistCount

            FROM ProviderImports pi

            INNER JOIN Providers p
                ON p.ProviderId = pi.ProviderId

            WHERE p.Name = $providerName

            LIMIT 1;
            """;

            command.Parameters.AddWithValue(
                "$providerName",
                providerName);

            using var reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            var providerOrdinal =
                reader.GetOrdinal(
                    "ProviderName");

            var lastImportedOrdinal =
                reader.GetOrdinal(
                    "LastImported");

            var trackCountOrdinal =
                reader.GetOrdinal(
                    "TrackCount");

            var playlistCountOrdinal =
                reader.GetOrdinal(
                    "PlaylistCount");

            DateTime lastImported =
                DateTime.MinValue;

            if (!reader.IsDBNull(
                    lastImportedOrdinal))
            {
                var value =
                    reader.GetValue(
                        lastImportedOrdinal);

                if (value is DateTime dateTime)
                {
                    lastImported =
                        dateTime;
                }
                else if (DateTime.TryParse(
                             Convert.ToString(value),
                             out var parsed))
                {
                    lastImported =
                        parsed;
                }
            }

            return new ProviderLibraryMetadata
            {
                ProviderName =
                    reader.GetString(
                        providerOrdinal),

                LastImported =
                    lastImported,

                TrackCount =
                    reader.GetInt32(
                        trackCountOrdinal),

                PlaylistCount =
                    reader.GetInt32(
                        playlistCountOrdinal)
            };
        });
    }

    // ============================================================
    // Bulk Library Operations
    // ============================================================

    /// <summary>
    /// Loads the SQLite library into a mutable list.
    /// </summary>
    public async Task<List<DJLMMediaItem>>
        LoadLibraryAsync()
    {
        return (
            await LoadAsync())
            .ToList();
    }

    /// <summary>
    /// Saves the supplied media collection into SQLite.
    ///
    /// Existing records are matched by file path.
    /// </summary>
    public async Task SaveLibraryAsync(
        IEnumerable<DJLMMediaItem> mediaItems)
    {
        ArgumentNullException.ThrowIfNull(
            mediaItems);

        var items =
            mediaItems
                .Where(x =>
                    x is not null &&
                    !string.IsNullOrWhiteSpace(x.FilePath))
                .ToList();

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                foreach (var mediaItem in items)
                {
                    var providerId =
                        GetOrCreateProvider(
                            connection,
                            transaction,
                            mediaItem.Provider);

                    var mediaId =
                        GetOrCreateMedia(
                            connection,
                            transaction,
                            mediaItem);

                    AddProviderIdentity(
                        connection,
                        transaction,
                        mediaId,
                        providerId);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    // ============================================================
    // Path Index
    // ============================================================

    /// <summary>
    /// Builds a fast lookup index of every file path currently
    /// stored in SQLite.
    /// </summary>
    public async Task<HashSet<string>>
        BuildPathIndexAsync()
    {
        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT FilePath
                FROM Media
                WHERE FilePath IS NOT NULL
                  AND FilePath <> '';
                """;

            using var reader =
                command.ExecuteReader();

            var paths =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                paths.Add(
                    reader.GetString(0));
            }

            return paths;
        });
    }

    // ============================================================
    // Statistics
    // ============================================================

    /// <summary>
    /// Returns the total number of physical Media records.
    /// </summary>
    public async Task<int> GetTrackCountAsync()
    {
        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT COUNT(*)
                FROM Media;
                """;

            return Convert.ToInt32(
                command.ExecuteScalar());
        });
    }

    /// <summary>
    /// Returns the total number of playlists stored in
    /// ProviderImports.
    ///
    /// Each provider contributes the playlist count from its
    /// most recent import.
    /// </summary>
    public async Task<int> GetPlaylistCountAsync()
    {
        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT COALESCE(
                    SUM(PlaylistCount),
                    0
                )
                FROM ProviderImports;
                """;

            return Convert.ToInt32(
                command.ExecuteScalar());
        });
    }

    // ============================================================
    // Clear
    // ============================================================

    /// <summary>
    /// Removes all media from the SQLite library.
    ///
    /// Provider identities, providers and provider import metadata
    /// are also removed.
    /// </summary>
    public async Task ClearAsync()
    {
        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                using var command =
                    connection.CreateCommand();

                command.Transaction =
                    transaction;

                command.CommandText =
                    """
                    DELETE FROM Media;
                    DELETE FROM Providers;
                    """;

                command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    // ============================================================
    // Clear Provider
    // ============================================================

    /// <summary>
    /// Removes the relationship between a provider and its media.
    ///
    /// Media records that are still associated with another provider
    /// remain in the library.
    ///
    /// Orphaned Media records are removed.
    ///
    /// The provider's import metadata is also removed.
    /// </summary>
    public async Task ClearProviderLibraryAsync(
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerName);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                using var command =
                    connection.CreateCommand();

                command.Transaction =
                    transaction;

                command.CommandText =
                    """
                    DELETE FROM MediaProviderIdentities
                    WHERE ProviderId =
                    (
                        SELECT ProviderId
                        FROM Providers
                        WHERE Name = $providerName
                    );
                    """;

                command.Parameters.AddWithValue(
                    "$providerName",
                    providerName);

                command.ExecuteNonQuery();

                RemoveOrphanedMedia(
                    connection,
                    transaction);

                command.Parameters.Clear();

                command.CommandText =
                    """
                    DELETE FROM ProviderImports
                    WHERE ProviderId =
                    (
                        SELECT ProviderId
                        FROM Providers
                        WHERE Name = $providerName
                    );
                    """;

                command.Parameters.AddWithValue(
                    "$providerName",
                    providerName);

                command.ExecuteNonQuery();

                command.Parameters.Clear();

                command.CommandText =
                    """
                    DELETE FROM Providers
                    WHERE Name = $providerName
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM MediaProviderIdentities mpi
                          WHERE mpi.ProviderId =
                                Providers.ProviderId
                      )
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM ProviderImports pi
                          WHERE pi.ProviderId =
                                Providers.ProviderId
                      );
                    """;

                command.Parameters.AddWithValue(
                    "$providerName",
                    providerName);

                command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    // ============================================================
    // SQLite Provider Helpers
    // ============================================================

    private static long GetOrCreateProvider(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            providerName =
                "Unknown";
        }

        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO Providers
            (
                Name
            )
            VALUES
            (
                $name
            )
            ON CONFLICT(Name)
            DO NOTHING;

            SELECT ProviderId
            FROM Providers
            WHERE Name = $name;
            """;

        command.Parameters.AddWithValue(
            "$name",
            providerName);

        var result =
            command.ExecuteScalar();

        if (result is null ||
            result == DBNull.Value)
        {
            throw new InvalidOperationException(
                $"Unable to resolve provider '{providerName}'.");
        }

        return Convert.ToInt64(
            result);
    }

    // ============================================================
    // SQLite Media Helpers
    // ============================================================

    private static string GetOrCreateMedia(
        SqliteConnection connection,
        SqliteTransaction transaction,
        DJLMMediaItem mediaItem)
    {
        //
        // First attempt to locate the existing physical track
        // by path.
        //
        using (var findCommand =
               connection.CreateCommand())
        {
            findCommand.Transaction =
                transaction;

            findCommand.CommandText =
                """
                SELECT MediaId
                FROM Media
                WHERE FilePath = $filePath
                LIMIT 1;
                """;

            findCommand.Parameters.AddWithValue(
                "$filePath",
                mediaItem.FilePath);

            var existing =
                findCommand.ExecuteScalar();

            if (existing is not null &&
                existing != DBNull.Value)
            {
                var mediaId =
                    Convert.ToString(existing)!;

                UpdateExistingMedia(
                    connection,
                    transaction,
                    mediaId,
                    mediaItem);

                return mediaId;
            }
        }

        //
        // No existing physical file.
        //
        var newMediaId =
            Guid.NewGuid().ToString();

        var now =
            DateTime.UtcNow.ToString(
                "O");

        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO Media
            (
                MediaId,
                TrackStatusId,
                MediaType,
                FilePath,
                FileSize,
                Artist,
                Title,
                Album,
                Genre,
                Year,
                BPM,
                MusicalKey,
                DurationSeconds,
                DateFirstSeen,
                DateLastModified,
                CreatedDate,
                LastModifiedDate
            )
            VALUES
            (
                $mediaId,
                1,
                $mediaType,
                $filePath,
                $fileSize,
                $artist,
                $title,
                $album,
                $genre,
                $year,
                $bpm,
                $musicalKey,
                $durationSeconds,
                $dateFirstSeen,
                $dateLastModified,
                $createdDate,
                $lastModifiedDate
            );
            """;

        AddMediaParameters(
            command,
            newMediaId,
            mediaItem,
            now,
            now);

        command.ExecuteNonQuery();

        return newMediaId;
    }

    private static void UpdateExistingMedia(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string mediaId,
        DJLMMediaItem mediaItem)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            UPDATE Media
            SET
                MediaType = $mediaType,
                FilePath = $filePath,
                FileSize = $fileSize,
                Artist = $artist,
                Title = $title,
                Album = $album,
                Genre = $genre,
                Year = $year,
                BPM = $bpm,
                MusicalKey = $musicalKey,
                DurationSeconds = $durationSeconds,
                DateFirstSeen = $dateFirstSeen,
                DateLastModified = $dateLastModified,
                LastModifiedDate = $lastModifiedDate

            WHERE MediaId = $mediaId;
            """;

        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$mediaType",
            mediaItem.MediaType ?? string.Empty);

        command.Parameters.AddWithValue(
            "$filePath",
            mediaItem.FilePath ?? string.Empty);

        command.Parameters.AddWithValue(
            "$fileSize",
            mediaItem.FileSize);

        command.Parameters.AddWithValue(
            "$artist",
            mediaItem.Artist ?? string.Empty);

        command.Parameters.AddWithValue(
            "$title",
            mediaItem.Title ?? string.Empty);

        command.Parameters.AddWithValue(
            "$album",
            mediaItem.Album ?? string.Empty);

        command.Parameters.AddWithValue(
            "$genre",
            mediaItem.Genre ?? string.Empty);

        command.Parameters.AddWithValue(
            "$year",
            mediaItem.Year.HasValue
                ? mediaItem.Year.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
           "$bpm",
            mediaItem.BPM.HasValue
                ? mediaItem.BPM.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$musicalKey",
            string.IsNullOrWhiteSpace(mediaItem.Key)
                ? string.Empty
                : mediaItem.Key);

        command.Parameters.AddWithValue(
            "$durationSeconds",
            mediaItem.Duration.HasValue
                ? mediaItem.Duration.Value.TotalSeconds
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$dateFirstSeen",
            mediaItem.DateFirstSeen.HasValue
                ? mediaItem.DateFirstSeen.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$dateLastModified",
            mediaItem.DateLastModified.HasValue
                ? mediaItem.DateLastModified.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$lastModifiedDate",
            DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();
    }

    private static void AddMediaParameters(
    SqliteCommand command,
    string mediaId,
    DJLMMediaItem mediaItem,
    string createdDate,
    string lastModifiedDate)
    {
        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$mediaType",
            mediaItem.MediaType ?? string.Empty);

        command.Parameters.AddWithValue(
            "$filePath",
            mediaItem.FilePath ?? string.Empty);

        command.Parameters.AddWithValue(
            "$fileSize",
            mediaItem.FileSize);

        command.Parameters.AddWithValue(
            "$artist",
            mediaItem.Artist ?? string.Empty);

        command.Parameters.AddWithValue(
            "$title",
            mediaItem.Title ?? string.Empty);

        command.Parameters.AddWithValue(
            "$album",
            mediaItem.Album ?? string.Empty);

        command.Parameters.AddWithValue(
            "$genre",
            mediaItem.Genre ?? string.Empty);

        command.Parameters.AddWithValue(
            "$year",
            mediaItem.Year.HasValue
                ? mediaItem.Year.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$bpm",
            mediaItem.BPM.HasValue
                ? mediaItem.BPM.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$musicalKey",
            string.IsNullOrWhiteSpace(mediaItem.Key)
                ? string.Empty
                : mediaItem.Key);

        command.Parameters.AddWithValue(
            "$durationSeconds",
            mediaItem.Duration.HasValue
                ? mediaItem.Duration.Value.TotalSeconds
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$dateFirstSeen",
            mediaItem.DateFirstSeen.HasValue
                ? mediaItem.DateFirstSeen.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$dateLastModified",
            mediaItem.DateLastModified.HasValue
                ? mediaItem.DateLastModified.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$createdDate",
            createdDate);

        command.Parameters.AddWithValue(
            "$lastModifiedDate",
            lastModifiedDate);
    }

    // ============================================================
    // Provider Identity
    // ============================================================

    private static void AddProviderIdentity(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string mediaId,
        long providerId)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO MediaProviderIdentities
            (
                MediaId,
                ProviderId,
                ProviderUniqueId,
                AudioFingerprint
            )
            VALUES
            (
                $mediaId,
                $providerId,
                NULL,
                NULL
            )
            ON CONFLICT
            (
                MediaId,
                ProviderId
            )
            DO NOTHING;
            """;

        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$providerId",
            providerId);

        command.ExecuteNonQuery();
    }

    private static void RemoveProviderIdentities(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long providerId)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            DELETE FROM MediaProviderIdentities
            WHERE ProviderId = $providerId;
            """;

        command.Parameters.AddWithValue(
            "$providerId",
            providerId);

        command.ExecuteNonQuery();
    }

    private static void RemoveOrphanedMedia(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            DELETE FROM Media
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM MediaProviderIdentities mpi
                WHERE mpi.MediaId = Media.MediaId
            );
            """;

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Reader
    // ============================================================

    private static DJLMMediaItem ReadMediaItem(
        SqliteDataReader reader)
    {
        var durationSeconds =
            reader.IsDBNull(
                reader.GetOrdinal(
                    "DurationSeconds"))
                ? (double?)null
                : reader.GetDouble(
                    reader.GetOrdinal(
                        "DurationSeconds"));

        var providerOrdinal =
            reader.GetOrdinal(
                "ProviderName");

        return new DJLMMediaItem
        {
            MediaId =
                reader.GetString(
                    reader.GetOrdinal(
                        "MediaId")),

            TrackStatusId =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "TrackStatusId")),

            Provider =
                reader.IsDBNull(providerOrdinal)
                    ? string.Empty
                    : reader.GetString(
                        providerOrdinal),

            MediaType =
                reader.GetString(
                    reader.GetOrdinal(
                        "MediaType")),

            FilePath =
                reader.GetString(
                    reader.GetOrdinal(
                        "FilePath")),

            FileSize =
                reader.GetInt64(
                    reader.GetOrdinal(
                        "FileSize")),

            Artist =
                reader.GetString(
                    reader.GetOrdinal(
                        "Artist")),

            Title =
                reader.GetString(
                    reader.GetOrdinal(
                        "Title")),

            Album =
                reader.GetString(
                    reader.GetOrdinal(
                        "Album")),

            Genre =
                reader.GetString(
                    reader.GetOrdinal(
                        "Genre")),

            Year =
                reader.IsDBNull(
                    reader.GetOrdinal(
                        "Year"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal(
                            "Year")),

            BPM =
                reader.IsDBNull(
                    reader.GetOrdinal(
                        "BPM"))
                    ? null
                    : reader.GetDouble(
                        reader.GetOrdinal(
                            "BPM")),

            Key =
                reader.GetString(
                    reader.GetOrdinal(
                        "MusicalKey")),

            Duration =
                durationSeconds.HasValue
                    ? TimeSpan.FromSeconds(
                        durationSeconds.Value)
                    : null,

            DateFirstSeen =
                ReadDateTime(
                    reader,
                    "DateFirstSeen"),

                DateLastModified =
                ReadDateTime(
                    reader,
                    "DateLastModified")
        };
    }

    private static DateTime? ReadDateTime(
        SqliteDataReader reader,
        string columnName)
    {
        var ordinal =
            reader.GetOrdinal(
                columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        var value =
            reader.GetValue(ordinal);

        if (value is DateTime dateTime)
            return dateTime;

        if (DateTime.TryParse(
                Convert.ToString(value),
                out var result))
        {
            return result;
        }

        return null;
    }
}