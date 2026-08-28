using System;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Data;

/// <summary>
/// Creates and maintains the initial DIASISS SQLite schema.
///
/// This class is deliberately independent of the existing JSON
/// LibraryRepository. During the persistence migration SQLite is
/// being built alongside the existing JSON library.
///
/// No existing library data is modified by this class.
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
    /// Creates the initial DIASISS SQLite schema if it does not
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
}