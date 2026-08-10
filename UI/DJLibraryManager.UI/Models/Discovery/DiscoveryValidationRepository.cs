using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models.Discovery;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DJLibraryManager.UI.Services.Discovery;

/// <summary>
/// Persists Discovery validation results.
/// </summary>
public sealed class DiscoveryValidationRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true
        };

    public DiscoveryValidationRepository()
    {
        ApplicationPaths.EnsureCreated();

        _filePath = ApplicationPaths.DiscoveryValidation;
    }

    /// <summary>
    /// Returns every persisted validation record.
    /// </summary>
    public IReadOnlyList<DiscoveryValidationRecord> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<List<DiscoveryValidationRecord>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    /// <summary>
    /// Returns the validation record for a media location.
    /// </summary>
    public DiscoveryValidationRecord? Get(string locationPath)
    {
        return Load().FirstOrDefault(x =>
            x.LocationPath.Equals(
                locationPath,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Saves or updates a validation record.
    /// </summary>
    public void Save(DiscoveryValidationRecord record)
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

    /// <summary>
    /// Replaces every validation record.
    /// </summary>
    public void SaveAll(IEnumerable<DiscoveryValidationRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var json = JsonSerializer.Serialize(
            records,
            _jsonOptions);

        File.WriteAllText(
            _filePath,
            json);
    }
}