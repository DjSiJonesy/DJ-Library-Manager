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
    ///
    /// Existing databases are upgraded in-place where
    /// required. Existing data is preserved.
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

            UpgradeFileChangesTable(
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
    /// FileChanges records changes made by workflow stages such
    /// as Improve and Structure.
    ///
    /// Physical file changes use:
    ///
    ///     OriginalPath
    ///     NewPath
    ///
    /// Provider database changes may additionally use:
    ///
    ///     ProviderId
    ///     ProviderDatabasePath
    ///     ProviderRecordData
    ///
    /// The provider-specific fields are nullable so existing
    /// physical file-change records remain unchanged.
    ///
    /// MediaId is the authoritative DIASISS GUID for the media
    /// record and is used to associate a change with the correct
    /// media item.
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
                    TEXT NOT NULL,

                ProviderId
                    INTEGER NULL,

                ProviderDatabasePath
                    TEXT NULL,

                ProviderRecordData
                    TEXT NULL
            );
            """;

        command.ExecuteNonQuery();

        // Do not create the FileChanges indexes here.
        //
        // Existing databases may have an older FileChanges table
        // without the provider-specific columns. The upgrade must
        // add those columns before the indexes reference them.
    }

    // ============================================================
    // File Changes Upgrade
    // ============================================================

    /// <summary>
    /// Upgrades an existing FileChanges table in-place.
    ///
    /// The application previously created FileChanges with only
    /// physical file-change fields. Provider database changes now
    /// require additional nullable fields.
    ///
    /// SQLite cannot add columns through CREATE TABLE IF NOT EXISTS,
    /// so existing databases are checked and upgraded here.
    ///
    /// Existing rows are preserved.
    /// </summary>
    private static void UpgradeFileChangesTable(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        if (!ColumnExists(
                connection,
                transaction,
                "FileChanges",
                "ProviderId"))
        {
            AddColumn(
                connection,
                transaction,
                "FileChanges",
                "ProviderId INTEGER NULL");
        }

        if (!ColumnExists(
                connection,
                transaction,
                "FileChanges",
                "ProviderDatabasePath"))
        {
            AddColumn(
                connection,
                transaction,
                "FileChanges",
                "ProviderDatabasePath TEXT NULL");
        }

        if (!ColumnExists(
                connection,
                transaction,
                "FileChanges",
                "ProviderRecordData"))
        {
            AddColumn(
                connection,
                transaction,
                "FileChanges",
                "ProviderRecordData TEXT NULL");
        }

        // The columns now definitely exist, so it is safe to create
        // the complete FileChanges index set.
        CreateFileChangeIndexes(
            connection,
            transaction);
    }

    private static bool ColumnExists(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string tableName,
        string columnName)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            $"""
            PRAGMA table_info({tableName});
            """;

        using var reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            var existingColumnName =
                reader.GetString(1);

            if (string.Equals(
                    existingColumnName,
                    columnName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddColumn(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string tableName,
        string columnDefinition)
    {
        using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            $"""
            ALTER TABLE {tableName}
            ADD COLUMN {columnDefinition};
            """;

        command.ExecuteNonQuery();
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
    /// locating and rolling back changes.
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

            CREATE INDEX IF NOT EXISTS IX_FileChanges_ProviderId
                ON FileChanges(
                    ProviderId
                );
            """;

        command.ExecuteNonQuery();
    }
}