using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models.Import;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DJLibraryManager.UI.Services.Import;

/// <summary>
/// Persists Media Location import history.
/// </summary>
public sealed class MediaImportRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true
        };

    public MediaImportRepository()
    {
        ApplicationPaths.EnsureCreated();

        _filePath = ApplicationPaths.MediaImports;
    }

    /// <summary>
    /// Returns every persisted import record.
    /// </summary>
    public IReadOnlyList<MediaImportRecord> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<List<MediaImportRecord>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    /// <summary>
    /// Returns the persisted record for a media location.
    /// </summary>
    public MediaImportRecord? Get(string locationPath)
    {
        return Load().FirstOrDefault(x =>
            x.LocationPath.Equals(
                locationPath,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Saves or updates a media location import record.
    /// </summary>
    public void Save(MediaImportRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var records = Load().ToList();

        var existing = records.FirstOrDefault(x =>
            x.LocationPath.Equals(
                record.LocationPath,
                StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            records.Remove(existing);
        }

        records.Add(record);

        var json = JsonSerializer.Serialize(
            records,
            _jsonOptions);

        File.WriteAllText(
            _filePath,
            json);
    }
}