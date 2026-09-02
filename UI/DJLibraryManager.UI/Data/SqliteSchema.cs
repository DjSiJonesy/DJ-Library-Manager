using System;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Data;

/// <summary>
/// Creates and maintains the DIASISS SQLite schema.
///
/// SQLite is the authoritative persistence store for the
/// provider-independent library and provider import metadata.
/// </summary>
public sealed class SqliteSchema
{
    private readonly SqliteDatabase _database;

    public SqliteSchema(
        SqliteDatabase database)
    {
        _database =
            database
            ?? throw new ArgumentNullException(
                nameof(database));
    }

    /// <summary>
    /// Creates the DIASISS SQLite schema if it does not
    /// already exist.
    /// </summary>
    public void EnsureCreated()
    {
        using var connection =
            _database.OpenConnection();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            CreateProvidersTable(
                connection,
                transaction);

            CreateMediaTable(
                connection,
                transaction);

            CreateMediaProviderIdentitiesTable(
                connection,
                transaction);

            CreateProviderImportsTable(
                connection,
                transaction);

            CreateFileChangesTable(
                connection,
                transaction);

            transaction.Commit();
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

    private static void CreateProvidersTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Providers
            (
                ProviderId INTEGER PRIMARY KEY AUTOINCREMENT,

                Name TEXT NOT NULL COLLATE NOCASE,

                CONSTRAINT UX_Providers_Name
                    UNIQUE (Name)
            );
            """;

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Media
    // ============================================================

    private static void CreateMediaTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Media
            (
                MediaId TEXT NOT NULL PRIMARY KEY,

                TrackStatusId INTEGER NOT NULL DEFAULT 1,

                MediaType TEXT NOT NULL DEFAULT '',

                FilePath TEXT NOT NULL DEFAULT '',

                FileSize INTEGER NOT NULL DEFAULT 0,

                Artist TEXT NOT NULL DEFAULT '',

                Title TEXT NOT NULL DEFAULT '',

                Album TEXT NOT NULL DEFAULT '',

                Genre TEXT NOT NULL DEFAULT '',

                Year INTEGER NULL,

                BPM REAL NULL,

                MusicalKey TEXT NOT NULL DEFAULT '',

                DurationSeconds REAL NULL,

                DateFirstSeen TEXT NULL,

                DateLastModified TEXT NULL,

                CreatedDate TEXT NOT NULL,

                LastModifiedDate TEXT NOT NULL
            );
            """;

        command.ExecuteNonQuery();

        CreateMediaIndexes(
            connection,
            transaction);
    }

    // ============================================================
    // Media Provider Identities
    // ============================================================

    private static void CreateMediaProviderIdentitiesTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS MediaProviderIdentities
            (
                MediaProviderIdentityId
                    INTEGER PRIMARY KEY AUTOINCREMENT,

                MediaId
                    TEXT NOT NULL,

                ProviderId
                    INTEGER NOT NULL,

                ProviderUniqueId
                    TEXT NULL,

                AudioFingerprint
                    TEXT NULL,

                CONSTRAINT FK_MediaProviderIdentities_Media
                    FOREIGN KEY (MediaId)
                    REFERENCES Media(MediaId)
                    ON DELETE CASCADE,

                CONSTRAINT FK_MediaProviderIdentities_Provider
                    FOREIGN KEY (ProviderId)
                    REFERENCES Providers(ProviderId)
                    ON DELETE CASCADE,

                CONSTRAINT UX_MediaProviderIdentities
                    UNIQUE (
                        MediaId,
                        ProviderId
                    )
            );
            """;

        command.ExecuteNonQuery();

        CreateMediaProviderIdentityIndexes(
            connection,
            transaction);
    }

    // ============================================================
    // Provider Import Metadata
    // ============================================================

    /// <summary>
    /// Stores the result of the most recent import for each
    /// provider.
    ///
    /// This replaces the previous JSON-based provider import
    /// metadata storage.
    /// </summary>
    private static void CreateProviderImportsTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS ProviderImports
            (
                ProviderImportId
                    INTEGER PRIMARY KEY AUTOINCREMENT,

                ProviderId
                    INTEGER NOT NULL,

                LastImported
                    TEXT NULL,

                TrackCount
                    INTEGER NOT NULL DEFAULT 0,

                PlaylistCount
                    INTEGER NOT NULL DEFAULT 0,

                CONSTRAINT FK_ProviderImports_Provider
                    FOREIGN KEY (ProviderId)
                    REFERENCES Providers(ProviderId)
                    ON DELETE CASCADE,

                CONSTRAINT UX_ProviderImports_Provider
                    UNIQUE (
                        ProviderId
                    )
            );
            """;

        command.ExecuteNonQuery();

        CreateProviderImportIndexes(
            connection,
            transaction);
    }

    // ============================================================
    // File Changes
    // ============================================================

    /// <summary>
    /// Creates the shared FileChanges table.
    ///
    /// FileChanges records physical file changes made by workflow
    /// stages such as Improve and Structure.
    ///
    /// Each operation receives its own OperationId and identifies
    /// the workflow stage which created the change.
    ///
    /// MediaId is the authoritative DIASISS GUID for the media
    /// record and is used to verify that a rollback is acting on
    /// the correct media item.
    ///
    /// OriginalPath and NewPath record the physical movement.
    ///
    /// Status records the outcome of the change.
    /// </summary>
    private static void CreateFileChangesTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS FileChanges
            (
                ChangeId
                    INTEGER PRIMARY KEY AUTOINCREMENT,

                OperationId
                    TEXT NOT NULL,

                Stage
                    TEXT NOT NULL,

                ChangeType
                    TEXT NOT NULL,

                MediaId
                    TEXT NOT NULL,

                OriginalPath
                    TEXT NOT NULL,

                NewPath
                    TEXT NOT NULL,

                Status
                    TEXT NOT NULL,

                ChangedDate
                    TEXT NOT NULL
            );
            """;

        command.ExecuteNonQuery();

        CreateFileChangeIndexes(
            connection,
            transaction);
    }

    // ============================================================
    // Media Indexes
    // ============================================================

    private static void CreateMediaIndexes(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE INDEX IF NOT EXISTS IX_Media_FilePath
                ON Media(FilePath);

            CREATE INDEX IF NOT EXISTS IX_Media_TrackStatusId
                ON Media(TrackStatusId);

            CREATE INDEX IF NOT EXISTS IX_Media_FilePath_Title
                ON Media(
                    FilePath,
                    Title
                );
            """;

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Provider Identity Indexes
    // ============================================================

    private static void CreateMediaProviderIdentityIndexes(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE INDEX IF NOT EXISTS IX_MediaProviderIdentities_Provider
                ON MediaProviderIdentities(ProviderId);

            CREATE INDEX IF NOT EXISTS IX_MediaProviderIdentities_ProviderUniqueId
                ON MediaProviderIdentities(
                    ProviderId,
                    ProviderUniqueId
                );

            CREATE INDEX IF NOT EXISTS IX_MediaProviderIdentities_AudioFingerprint
                ON MediaProviderIdentities(
                    AudioFingerprint
                );
            """;

        command.ExecuteNonQuery();
    }

    // ============================================================
    // Provider Import Indexes
    // ============================================================

    private static void CreateProviderImportIndexes(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE INDEX IF NOT EXISTS IX_ProviderImports_Provider
                ON ProviderImports(ProviderId);
            """;

        command.ExecuteNonQuery();
    }

    // ============================================================
    // File Change Indexes
    // ============================================================

    /// <summary>
    /// Creates indexes used by Improve and Structure when
    /// locating and rolling back file changes.
    /// </summary>
    private static void CreateFileChangeIndexes(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            CREATE INDEX IF NOT EXISTS IX_FileChanges_OperationId
                ON FileChanges(
                    OperationId
                );

            CREATE INDEX IF NOT EXISTS IX_FileChanges_Stage
                ON FileChanges(
                    Stage
                );

            CREATE INDEX IF NOT EXISTS IX_FileChanges_MediaId
                ON FileChanges(
                    MediaId
                );

            CREATE INDEX IF NOT EXISTS IX_FileChanges_Status
                ON FileChanges(
                    Status
                );

            CREATE INDEX IF NOT EXISTS IX_FileChanges_Stage_OperationId
                ON FileChanges(
                    Stage,
                    OperationId
                );
            """;

        command.ExecuteNonQuery();
    }
}