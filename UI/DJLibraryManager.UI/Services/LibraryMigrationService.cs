using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Data;
using DJLibraryManager.UI.Models.Media;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Migrates the existing JSON media library into the DIASISS SQLite database.
///
/// This is a one-time migration utility used during the persistence migration.
/// The existing JSON library is treated as the source of truth and is never
/// modified by this service.
///
/// Multiple legacy provider records that refer to the same physical file are
/// consolidated into one DIASISS Media record. Each provider is then attached
/// to that Media record through MediaProviderIdentities.
///
/// The DIASISS MediaId is permanent and represents the logical DIASISS media
/// record, independently of provider identity.
/// </summary>
public sealed class LibraryMigrationService
{
    private readonly LibraryRepository _libraryRepository;
    private readonly SqliteDatabase _database;
    private readonly SqliteSchema _schema;

    public LibraryMigrationService(
        LibraryRepository libraryRepository,
        SqliteDatabase database,
        SqliteSchema schema)
    {
        _libraryRepository =
            libraryRepository
            ?? throw new ArgumentNullException(
                nameof(libraryRepository));

        _database =
            database
            ?? throw new ArgumentNullException(
                nameof(database));

        _schema =
            schema
            ?? throw new ArgumentNullException(
                nameof(schema));
    }

    /// <summary>
    /// Migrates the existing JSON library into SQLite.
    ///
    /// The JSON library remains untouched.
    ///
    /// Records with the same physical FilePath are represented by one
    /// DIASISS Media record. Provider relationships are stored separately.
    /// </summary>
    public async Task<LibraryMigrationResult> MigrateAsync()
    {
        _schema.EnsureCreated();

        var mediaItems =
            await _libraryRepository.LoadAsync();

        if (mediaItems.Count == 0)
        {
            return new LibraryMigrationResult(
                RecordsRead: 0,
                MediaRecordsCreated: 0,
                ProviderIdentitiesCreated: 0,
                RecordsSkipped: 0);
        }

        using var connection =
            _database.OpenConnection();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            var providerIds =
                new Dictionary<string, long>(
                    StringComparer.OrdinalIgnoreCase);

            //
            // Maps the normalised physical file path to the DIASISS
            // MediaId created for that physical file.
            //
            var mediaIdsByPath =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            var createdAt =
                DateTime.Now;

            var mediaRecordsCreated = 0;
            var providerIdentitiesCreated = 0;
            var skipped = 0;

            foreach (var mediaItem in mediaItems)
            {
                if (mediaItem is null)
                {
                    skipped++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        mediaItem.Provider))
                {
                    skipped++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        mediaItem.FilePath))
                {
                    skipped++;
                    continue;
                }

                var filePath =
                    NormalizeFilePath(
                        mediaItem.FilePath);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    skipped++;
                    continue;
                }

                var providerId =
                    GetOrCreateProvider(
                        connection,
                        transaction,
                        mediaItem.Provider,
                        providerIds);

                //
                // ----------------------------------------------------
                // Resolve the DIASISS Media record.
                // ----------------------------------------------------
                //
                // Multiple provider records for the same physical
                // file share the same DIASISS MediaId.
                //

                if (!mediaIdsByPath.TryGetValue(
                        filePath,
                        out var mediaId))
                {
                    mediaId =
                        Guid.NewGuid().ToString();

                    var trackStatusId =
                        DetermineTrackStatus(
                            filePath);

                    InsertMedia(
                        connection,
                        transaction,
                        mediaId,
                        mediaItem,
                        filePath,
                        trackStatusId,
                        createdAt);

                    mediaIdsByPath[filePath] =
                        mediaId;

                    mediaRecordsCreated++;
                }

                //
                // ----------------------------------------------------
                // Add the provider identity.
                // ----------------------------------------------------
                //
                // The legacy JSON model does not contain a native
                // ProviderUniqueId, so these fields deliberately
                // remain NULL during migration.
                //

                if (ProviderIdentityExists(
                        connection,
                        transaction,
                        mediaId,
                        providerId))
                {
                    continue;
                }

                InsertMediaProviderIdentity(
                    connection,
                    transaction,
                    mediaId,
                    providerId);

                providerIdentitiesCreated++;
            }

            transaction.Commit();

            return new LibraryMigrationResult(
                RecordsRead: mediaItems.Count,
                MediaRecordsCreated:
                    mediaRecordsCreated,
                ProviderIdentitiesCreated:
                    providerIdentitiesCreated,
                RecordsSkipped:
                    skipped);
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }

    // ============================================================
    // Providers
    // ============================================================

    private static long GetOrCreateProvider(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string providerName,
        IDictionary<string, long> providerIds)
    {
        if (providerIds.TryGetValue(
                providerName,
                out var cachedProviderId))
        {
            return cachedProviderId;
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
                $"Unable to resolve provider '{providerName}' after insertion.");
        }

        var providerId =
            Convert.ToInt64(result);

        providerIds[providerName] =
            providerId;

        return providerId;
    }

    // ============================================================
    // Media
    // ============================================================

    private static void InsertMedia(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string mediaId,
        DJLMMediaItem mediaItem,
        string filePath,
        int trackStatusId,
        DateTime createdAt)
    {
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
                $trackStatusId,
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

        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$trackStatusId",
            trackStatusId);

        command.Parameters.AddWithValue(
            "$mediaType",
            mediaItem.MediaType ?? string.Empty);

        command.Parameters.AddWithValue(
            "$filePath",
            filePath);

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
            mediaItem.Key ?? string.Empty);

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
            createdAt);

        command.Parameters.AddWithValue(
            "$lastModifiedDate",
            createdAt);

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Provider Identity
    // ============================================================

    private static bool ProviderIdentityExists(
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
            SELECT COUNT(*)
            FROM MediaProviderIdentities
            WHERE MediaId = $mediaId
              AND ProviderId = $providerId;
            """;

        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$providerId",
            providerId);

        var result =
            command.ExecuteScalar();

        return Convert.ToInt32(result) > 0;
    }

    private static void InsertMediaProviderIdentity(
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
            );
            """;

        command.Parameters.AddWithValue(
            "$mediaId",
            mediaId);

        command.Parameters.AddWithValue(
            "$providerId",
            providerId);

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Track Status
    // ============================================================

    private static int DetermineTrackStatus(
        string filePath)
    {
        try
        {
            return File.Exists(filePath)
                ? 1
                : 2;
        }
        catch
        {
            return 2;
        }
    }

    // ============================================================
    // File Path
    // ============================================================

    private static string NormalizeFilePath(
        string filePath)
    {
        var trimmed =
            filePath.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            return string.Empty;

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch
        {
            return trimmed;
        }
    }
}

/// <summary>
/// Result returned by the JSON-to-SQLite migration.
/// </summary>
public sealed record LibraryMigrationResult(
    int RecordsRead,
    int MediaRecordsCreated,
    int ProviderIdentitiesCreated,
    int RecordsSkipped);