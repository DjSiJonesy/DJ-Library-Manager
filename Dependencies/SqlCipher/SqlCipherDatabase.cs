using System.Data;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace DJLM.SqlCipher;

/// <summary>
/// Provides helper methods for opening and querying SQLCipher databases.
/// </summary>
public static class SqlCipherDatabase
{
    private static bool _initialised = false;

    /// <summary>
    /// Opens a SQLCipher database and applies the encryption key.
    /// </summary>
    /// <param name="databasePath">
    /// Full path to the database.
    /// </param>
    /// <param name="key">
    /// SQLCipher encryption key.
    /// </param>
    /// <returns>
    /// An open SqliteConnection.
    /// </returns>
    public static SqliteConnection Open(
        string databasePath,
        string key)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentNullException(nameof(databasePath));

        if (!File.Exists(databasePath))
            throw new FileNotFoundException(
                "Database file not found.",
                databasePath);

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        //
        // Initialise SQLitePCL once
        //
        if (!_initialised)
        {
            Batteries.Init();
            _initialised = true;
        }

        //
        // Open the database
        //
        var connection = new SqliteConnection(
            $"Data Source={databasePath}");

        connection.Open();

        //
        // Apply SQLCipher key
        //
            using (var command = connection.CreateCommand())
            {
                // Escape any single quotes in the key.
                var escapedKey = key.Replace("'", "''");

                command.CommandText = $"PRAGMA key = '{escapedKey}';";

                command.ExecuteNonQuery();
            }

        return connection;
    }

    /// <summary>
    /// Executes a SQL query and returns the results.
    /// </summary>
    public static DataTable Query(
        SqliteConnection connection,
        string sql)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentNullException(nameof(sql));

        using var command = connection.CreateCommand();

        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        var table = new DataTable();

        table.Load(reader);

        return table;
    }

    /// <summary>
    /// Closes and disposes a SQLCipher database connection.
    /// </summary>
    public static void Close(
        SqliteConnection? connection)
    {
        if (connection == null)
            return;

        connection.Close();
        connection.Dispose();
    }
}