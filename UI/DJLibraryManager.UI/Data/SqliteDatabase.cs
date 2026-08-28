using System;
using System.IO;

using Microsoft.Data.Sqlite;

namespace DJLibraryManager.UI.Data;

/// <summary>
/// Provides the connection to the DIASISS SQLite database.
///
/// This class is deliberately limited to database infrastructure.
/// It does not create tables and does not contain library logic.
///
/// The existing JSON library remains the active persistence mechanism
/// while the SQLite migration is being developed.
/// </summary>
public sealed class SqliteDatabase
{
    private readonly string _databasePath;

    /// <summary>
    /// Gets the full path to the DIASISS SQLite database.
    /// </summary>
    public string DatabasePath =>
        _databasePath;

    /// <summary>
    /// Creates the SQLite database infrastructure.
    ///
    /// The database file is created when the database connection
    /// is opened. No tables are created at this stage.
    /// </summary>
    public SqliteDatabase(
        string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException(
                "A SQLite database path is required.",
                nameof(databasePath));
        }

        _databasePath =
            Path.GetFullPath(
                databasePath);

        var directory =
            Path.GetDirectoryName(
                _databasePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(
                directory);
        }

        // --------------------------------------------------------
        // Step 3:
        // Open the database once so that SQLite creates the
        // database file if it does not already exist.
        //
        // No tables are created here.
        // --------------------------------------------------------

        using var connection =
            OpenConnection();
    }

    /// <summary>
    /// Opens a connection to the DIASISS SQLite database.
    /// </summary>
    public SqliteConnection OpenConnection()
    {
        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    _databasePath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate
            }.ToString();

        var connection =
            new SqliteConnection(
                connectionString);

        connection.Open();

        return connection;
    }
}