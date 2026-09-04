using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DJLibraryManager.UI.Data;
using DJLibraryManager.UI.Models;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Provides persistence for changes made by the Improve and Structure
/// workflow stages.
///
/// FileChanges can record both:
///
///     Physical file changes
///     Provider database changes
///
/// Physical file changes use OriginalPath and NewPath.
///
/// Provider database changes additionally record the ProviderId,
/// ProviderDatabasePath and ProviderRecordData required to identify
/// and recover the provider database change.
///
/// This repository records and retrieves change information only.
/// It does not perform physical file operations or provider database
/// operations.
/// </summary>
public sealed class FileChangeRepository
{
    private readonly SqliteDatabase _database;

    public FileChangeRepository(
        SqliteDatabase database)
    {
        ArgumentNullException.ThrowIfNull(
            database);

        _database =
            database;
    }

    // ============================================================
    // Record Physical File Change
    // ============================================================

    /// <summary>
    /// Records a physical file change.
    ///
    /// This method is used by operations such as Duplicate removal
    /// and Structure.
    ///
    /// MediaId is the authoritative DIASISS media GUID.
    ///
    /// OperationId groups all changes belonging to one Improve or
    /// Structure operation.
    /// </summary>
    public async Task RecordChangeAsync(
        string operationId,
        string stage,
        string changeType,
        string mediaId,
        string originalPath,
        string newPath,
        string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operationId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            stage);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            changeType);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            mediaId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            originalPath);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            newPath);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            status);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO FileChanges
                (
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate
                )
                VALUES
                (
                    $operationId,
                    $stage,
                    $changeType,
                    $mediaId,
                    $originalPath,
                    $newPath,
                    NULL,
                    NULL,
                    NULL,
                    $status,
                    $changedDate
                );
                """;

            command.Parameters.AddWithValue(
                "$operationId",
                operationId);

            command.Parameters.AddWithValue(
                "$stage",
                stage);

            command.Parameters.AddWithValue(
                "$changeType",
                changeType);

            command.Parameters.AddWithValue(
                "$mediaId",
                mediaId);

            command.Parameters.AddWithValue(
                "$originalPath",
                originalPath);

            command.Parameters.AddWithValue(
                "$newPath",
                newPath);

            command.Parameters.AddWithValue(
                "$status",
                status);

            command.Parameters.AddWithValue(
                "$changedDate",
                DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        });
    }

    // ============================================================
    // Record Provider Database Change
    // ============================================================

    /// <summary>
    /// Records a provider database change.
    ///
    /// Provider database changes are separate from physical file
    /// movements. The provider identity, provider database path and
    /// original provider record data are retained so the operation
    /// can be recovered later.
    /// </summary>
    public async Task RecordProviderChangeAsync(
        string operationId,
        string stage,
        string changeType,
        string mediaId,
        string originalPath,
        long providerId,
        string providerDatabasePath,
        string providerRecordData,
        string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operationId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            stage);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            changeType);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            mediaId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            originalPath);

        if (providerId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(providerId),
                "A valid ProviderId is required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerDatabasePath);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerRecordData);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            status);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO FileChanges
                (
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate
                )
                VALUES
                (
                    $operationId,
                    $stage,
                    $changeType,
                    $mediaId,
                    $originalPath,
                    '',
                    $providerId,
                    $providerDatabasePath,
                    $providerRecordData,
                    $status,
                    $changedDate
                );
                """;

            command.Parameters.AddWithValue(
                "$operationId",
                operationId);

            command.Parameters.AddWithValue(
                "$stage",
                stage);

            command.Parameters.AddWithValue(
                "$changeType",
                changeType);

            command.Parameters.AddWithValue(
                "$mediaId",
                mediaId);

            command.Parameters.AddWithValue(
                "$originalPath",
                originalPath);

            command.Parameters.AddWithValue(
                "$providerId",
                providerId);

            command.Parameters.AddWithValue(
                "$providerDatabasePath",
                providerDatabasePath);

            command.Parameters.AddWithValue(
                "$providerRecordData",
                providerRecordData);

            command.Parameters.AddWithValue(
                "$status",
                status);

            command.Parameters.AddWithValue(
                "$changedDate",
                DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        });
    }

    // ============================================================
    // Get Operation Changes
    // ============================================================

    /// <summary>
    /// Returns all changes belonging to a specific operation.
    /// </summary>
    public async Task<IReadOnlyList<FileChangeRecord>>
        GetOperationChangesAsync(
            string operationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operationId);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    ChangeId,
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate

                FROM FileChanges

                WHERE OperationId = $operationId

                ORDER BY ChangeId;
                """;

            command.Parameters.AddWithValue(
                "$operationId",
                operationId);

            return ReadChanges(
                command);
        });
    }

    // ============================================================
    // Get Stage Changes
    // ============================================================

    /// <summary>
    /// Returns all changes recorded by a particular workflow stage.
    ///
    /// Improve and Structure therefore remain independently
    /// addressable even though they share the same table.
    /// </summary>
    public async Task<IReadOnlyList<FileChangeRecord>>
        GetStageChangesAsync(
            string stage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            stage);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    ChangeId,
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate

                FROM FileChanges

                WHERE Stage = $stage

                ORDER BY ChangeId;
                """;

            command.Parameters.AddWithValue(
                "$stage",
                stage);

            return ReadChanges(
                command);
        });
    }

    // ============================================================
    // Get Latest Operation
    // ============================================================

    /// <summary>
    /// Returns the most recent operation for a workflow stage.
    ///
    /// Only changes belonging to the requested stage are considered.
    /// </summary>
    public async Task<IReadOnlyList<FileChangeRecord>>
        GetLatestOperationAsync(
            string stage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            stage);

        return await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    ChangeId,
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate

                FROM FileChanges

                WHERE Stage = $stage
                  AND OperationId =
                  (
                      SELECT OperationId
                      FROM FileChanges
                      WHERE Stage = $stage
                      ORDER BY ChangeId DESC
                      LIMIT 1
                  )

                ORDER BY ChangeId;
                """;

            command.Parameters.AddWithValue(
                "$stage",
                stage);

            return ReadChanges(
                command);
        });
    }

    // ============================================================
    // Get Change By ID
    // ============================================================

    /// <summary>
    /// Returns a single change record.
    /// </summary>
    public async Task<FileChangeRecord?>
        GetChangeAsync(
            long changeId)
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
                    ChangeId,
                    OperationId,
                    Stage,
                    ChangeType,
                    MediaId,
                    OriginalPath,
                    NewPath,
                    ProviderId,
                    ProviderDatabasePath,
                    ProviderRecordData,
                    Status,
                    ChangedDate

                FROM FileChanges

                WHERE ChangeId = $changeId

                LIMIT 1;
                """;

            command.Parameters.AddWithValue(
                "$changeId",
                changeId);

            using var reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            return ReadChange(
                reader);
        });
    }

    // ============================================================
    // Update Status
    // ============================================================

    /// <summary>
    /// Updates the status of an existing change.
    /// </summary>
    public async Task UpdateStatusAsync(
        long changeId,
        string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            status);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                UPDATE FileChanges

                SET
                    Status = $status

                WHERE ChangeId = $changeId;
                """;

            command.Parameters.AddWithValue(
                "$status",
                status);

            command.Parameters.AddWithValue(
                "$changeId",
                changeId);

            command.ExecuteNonQuery();
        });
    }

    // ============================================================
    // Delete Operation
    // ============================================================

    /// <summary>
    /// Deletes all records belonging to one operation.
    ///
    /// This is intentionally provided for future maintenance and
    /// recovery-management operations. Normal rollback should not
    /// automatically delete the audit history.
    /// </summary>
    public async Task DeleteOperationAsync(
        string operationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operationId);

        await Task.Run(() =>
        {
            using var connection =
                _database.OpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                DELETE FROM FileChanges
                WHERE OperationId = $operationId;
                """;

            command.Parameters.AddWithValue(
                "$operationId",
                operationId);

            command.ExecuteNonQuery();
        });
    }

    // ============================================================
    // Read Changes
    // ============================================================

    private static IReadOnlyList<FileChangeRecord>
        ReadChanges(
            SqliteCommand command)
    {
        using var reader =
            command.ExecuteReader();

        var results =
            new List<FileChangeRecord>();

        while (reader.Read())
        {
            results.Add(
                ReadChange(
                    reader));
        }

        return results;
    }

    // ============================================================
    // Read Change
    // ============================================================

    private static FileChangeRecord ReadChange(
        SqliteDataReader reader)
    {
        var providerIdOrdinal =
            reader.GetOrdinal(
                "ProviderId");

        var providerDatabasePathOrdinal =
            reader.GetOrdinal(
                "ProviderDatabasePath");

        var providerRecordDataOrdinal =
            reader.GetOrdinal(
                "ProviderRecordData");

        return new FileChangeRecord
        {
            ChangeId =
                reader.GetInt64(
                    reader.GetOrdinal(
                        "ChangeId")),

            OperationId =
                reader.GetString(
                    reader.GetOrdinal(
                        "OperationId")),

            Stage =
                reader.GetString(
                    reader.GetOrdinal(
                        "Stage")),

            ChangeType =
                reader.GetString(
                    reader.GetOrdinal(
                        "ChangeType")),

            MediaId =
                reader.GetString(
                    reader.GetOrdinal(
                        "MediaId")),

            OriginalPath =
                reader.GetString(
                    reader.GetOrdinal(
                        "OriginalPath")),

            NewPath =
                reader.GetString(
                    reader.GetOrdinal(
                        "NewPath")),

            ProviderId =
                reader.IsDBNull(
                    providerIdOrdinal)
                    ? null
                    : reader.GetInt64(
                        providerIdOrdinal),

            ProviderDatabasePath =
                reader.IsDBNull(
                    providerDatabasePathOrdinal)
                    ? null
                    : reader.GetString(
                        providerDatabasePathOrdinal),

            ProviderRecordData =
                reader.IsDBNull(
                    providerRecordDataOrdinal)
                    ? null
                    : reader.GetString(
                        providerRecordDataOrdinal),

            Status =
                reader.GetString(
                    reader.GetOrdinal(
                        "Status")),

            ChangedDate =
                reader.GetString(
                    reader.GetOrdinal(
                        "ChangedDate"))
        };
    }
}