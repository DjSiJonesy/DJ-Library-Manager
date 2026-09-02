using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DJLibraryManager.UI.Data;
using DJLibraryManager.UI.Models;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Provides persistence for physical file changes made by the
/// Improve and Structure workflow stages.
///
/// FileChanges is shared by both workflow stages, but every record
/// identifies the stage which created it. This allows Improve and
/// Structure to maintain completely independent rollback operations.
///
/// This repository records and retrieves change information only.
/// It does not perform physical file operations.
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
    // Record Change
    // ============================================================

    /// <summary>
    /// Records a physical file change.
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
    ///
    /// This is useful when a change initially needs to be recorded
    /// and its final outcome is determined afterwards.
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