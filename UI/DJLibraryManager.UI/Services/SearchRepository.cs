using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Models.Search;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Stores and retrieves the latest Search state.
///
/// Search persistence is independent of the Search workspace UI.
/// It allows completed searches and their results to survive
/// application restarts.
/// </summary>
public sealed class SearchRepository
{
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

    /// <summary>
    /// The latest completed Search state.
    /// </summary>
    public SearchState? CurrentSearch { get; private set; }

    /// <summary>
    /// Returns true if a Search state exists.
    /// </summary>
    public bool HasSearch =>
        CurrentSearch is not null;

    /// <summary>
    /// Saves the latest Search state.
    /// </summary>
    public void Save(SearchState state)
    {
        CurrentSearch = state;

        Directory.CreateDirectory(
            ApplicationPaths.Search);

        var document =
            new SearchDocument
            {
                Version = 1,
                Search = state
            };

        File.WriteAllText(
            ApplicationPaths.LatestSearch,
            JsonSerializer.Serialize(
                document,
                JsonOptions));
    }

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

        var json =
            File.ReadAllText(
                ApplicationPaths.LatestSearch);

        var document =
            JsonSerializer.Deserialize<SearchDocument>(
                json,
                JsonOptions);

        if (document is null)
            return;

        switch (document.Version)
        {
            case 1:
                CurrentSearch =
                    document.Search;
                break;

            default:
                CurrentSearch = null;
                break;
        }
    }

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