using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models.Search;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Stores and retrieves the latest Search state.
///
/// Search persistence is independent of the Search workspace UI.
/// It allows completed searches and their results to survive
/// application restarts, including interrupted Search All runs.
/// </summary>
public sealed class SearchRepository
{
    private const int CurrentVersion = 2;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true,

            // Populate existing collection instances when
            // deserialising objects with getter-only collections.
            //
            // This is required for SearchIssue.Results and
            // SearchIssue.RelatedFilePaths.
            PreferredObjectCreationHandling =
                JsonObjectCreationHandling.Populate
        };

    public SearchRepository()
    {
        Load();
    }

    // ============================================================
    // Current Search
    // ============================================================

    /// <summary>
    /// The latest Search state.
    /// </summary>
    public SearchState? CurrentSearch { get; private set; }

    /// <summary>
    /// Returns true if a Search state exists.
    /// </summary>
    public bool HasSearch =>
        CurrentSearch is not null;

    // ============================================================
    // Save
    // ============================================================

    /// <summary>
    /// Saves the latest Search state.
    /// </summary>
    public void Save(
        SearchState state)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        CurrentSearch =
            state;

        Directory.CreateDirectory(
            ApplicationPaths.Search);

        var document =
            new SearchDocument
            {
                Version =
                    CurrentVersion,

                Search =
                    state
            };

        File.WriteAllText(
            ApplicationPaths.LatestSearch,
            JsonSerializer.Serialize(
                document,
                JsonOptions));
    }

    // ============================================================
    // Load
    // ============================================================

    /// <summary>
    /// Loads the latest Search state from disk.
    /// </summary>
    private void Load()
    {
        if (!File.Exists(
                ApplicationPaths.LatestSearch))
        {
            return;
        }

        try
        {
            var json =
                File.ReadAllText(
                    ApplicationPaths.LatestSearch);

            var document =
                JsonSerializer.Deserialize<SearchDocument>(
                    json,
                    JsonOptions);

            if (document is null)
            {
                CurrentSearch = null;
                return;
            }

            switch (document.Version)
            {
                // ------------------------------------------------
                // Version 1
                //
                // Version 1 did not contain SearchRun information.
                // The existing Search results remain valid.
                // ------------------------------------------------

                case 1:
                    CurrentSearch =
                        document.Search;

                    break;

                // ------------------------------------------------
                // Version 2
                //
                // Version 2 supports resumable Search All.
                // ------------------------------------------------

                case 2:
                    CurrentSearch =
                        document.Search;

                    break;

                // ------------------------------------------------
                // Unknown version
                // ------------------------------------------------

                default:
                    CurrentSearch = null;
                    break;
            }
        }
        catch
        {
            // A corrupt or unreadable Search file must not prevent
            // the application from starting.
            CurrentSearch = null;
        }
    }

    // ============================================================
    // Clear
    // ============================================================

    /// <summary>
    /// Clears the current Search state.
    /// </summary>
    public void Clear()
    {
        CurrentSearch = null;

        if (File.Exists(
                ApplicationPaths.LatestSearch))
        {
            File.Delete(
                ApplicationPaths.LatestSearch);
        }
    }
}