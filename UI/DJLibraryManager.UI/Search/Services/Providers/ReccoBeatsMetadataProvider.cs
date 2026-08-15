using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Searches ReccoBeats for possible metadata matches.
///
/// ReccoBeats is used as an independent discovery and audio
/// evidence source. It does not consume results from other
/// metadata providers and does not modify the DIASISS library.
///
/// Search flow:
///
/// Artist + Title
///      ↓
/// ReccoBeats Search
///      ↓
/// Candidate Track IDs
///      ↓
/// Track Details
///      ↓
/// Audio Features
///      ↓
/// MetadataSearchProviderResult
///
/// ReccoBeats BPM and key values are provider evidence only.
/// DIASISS analysis is responsible for deciding whether those
/// values agree with the existing library metadata or other
/// provider evidence.
/// </summary>
public sealed class ReccoBeatsMetadataProvider
    : IMetadataSearchProvider
{
    private const string BaseUrl =
        "https://api.reccobeats.com/v1";

    private const int SearchSize = 10;

    private readonly HttpClient _httpClient;

    public ReccoBeatsMetadataProvider()
    {
        _httpClient =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromSeconds(30)
            };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "DIASISS/1.0");
    }

    public string Name =>
        "ReccoBeats";

    // ============================================================
    // Search
    // ============================================================

    public async Task<IReadOnlyList<MetadataSearchProviderResult>>
        SearchAsync(
            MetadataSearchRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        //
        // ReccoBeats requires useful search information.
        //

        if (string.IsNullOrWhiteSpace(request.Title) &&
            string.IsNullOrWhiteSpace(request.Artist))
        {
            return Array.Empty<
                MetadataSearchProviderResult>();
        }

        try
        {
            var searchResults =
                await SearchTracksAsync(
                    request,
                    cancellationToken);

            if (searchResults.Count == 0)
            {
                return Array.Empty<
                    MetadataSearchProviderResult>();
            }

            var results =
                new List<MetadataSearchProviderResult>();

            foreach (var searchResult in searchResults)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result =
                    await BuildProviderResultAsync(
                        searchResult,
                        request,
                        cancellationToken);

                if (result is not null)
                {
                    results.Add(result);
                }
            }

            return results;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            //
            // A provider failure must not prevent the other
            // metadata providers from returning their results.
            //

            return Array.Empty<
                MetadataSearchProviderResult>();
        }
    }

    // ============================================================
    // ReccoBeats Search
    // ============================================================

    private async Task<List<JsonElement>>
        SearchTracksAsync(
            MetadataSearchRequest request,
            CancellationToken cancellationToken)
    {
        var searchText =
            request.Title?.Trim() ?? string.Empty;

        var artist =
            request.Artist?.Trim() ?? string.Empty;

        //
        // If the title is unavailable, use the artist as the
        // search text so that the provider can still attempt
        // discovery.
        //

        if (string.IsNullOrWhiteSpace(searchText))
        {
            searchText = artist;
        }

        var query =
            new List<string>
            {
                $"searchText={Uri.EscapeDataString(searchText)}",
                $"size={SearchSize}",
                "page=0"
            };

        if (!string.IsNullOrWhiteSpace(artist))
        {
            query.Add(
                $"artist={Uri.EscapeDataString(artist)}");
        }

        var url =
            $"{BaseUrl}/track/search?" +
            string.Join("&", query);

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        try
        {
            using var document =
                JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "content",
                    out var content) ||
                content.ValueKind !=
                    JsonValueKind.Array)
            {
                return [];
            }

            return content
                .EnumerateArray()
                .Select(
                    result => result.Clone())
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    // ============================================================
    // Candidate Enrichment
    // ============================================================

    private async Task<MetadataSearchProviderResult?>
        BuildProviderResultAsync(
            JsonElement searchResult,
            MetadataSearchRequest request,
            CancellationToken cancellationToken)
    {
        if (!searchResult.TryGetProperty(
                "id",
                out var idElement))
        {
            return null;
        }

        var trackId =
            idElement.GetString();

        if (string.IsNullOrWhiteSpace(trackId))
        {
            return null;
        }

        //
        // Retrieve the more complete Track Details response.
        //

        var details =
            await GetTrackDetailsAsync(
                trackId,
                cancellationToken);

        //
        // Retrieve ReccoBeats Audio Features.
        //

        var features =
            await GetAudioFeaturesAsync(
                trackId,
                cancellationToken);

        //
        // Use the search response as a fallback when one of the
        // enrichment calls does not return data.
        //

        var artist =
            GetArtistNames(
                details,
                searchResult);

        var title =
            GetString(
                details,
                "trackTitle")
            ?? GetString(
                searchResult,
                "trackTitle")
            ?? string.Empty;

        var duration =
            GetDuration(
                details,
                searchResult);

        var bpm =
            GetNullableDouble(
                features,
                "tempo");

        var key =
            BuildMusicalKey(
                features);

        var confidence =
            CalculateConfidence(
                request,
                artist,
                title,
                duration);

        var reason =
            BuildMatchReason(
                request,
                artist,
                title,
                duration,
                bpm,
                key);

        return new MetadataSearchProviderResult
        {
            Source =
                Name,

            ExternalId =
                trackId,

            Artist =
                artist,

            Title =
                title,

            Album =
                string.Empty,

            Genre =
                string.Empty,

            Year =
                null,

            ReleaseYear =
                null,

            BPM =
                bpm,

            Key =
                key,

            Duration =
                duration,

            Confidence =
                confidence,

            MatchReason =
                reason
        };
    }

    // ============================================================
    // Track Details
    // ============================================================

    private async Task<JsonElement?>
        GetTrackDetailsAsync(
            string trackId,
            CancellationToken cancellationToken)
    {
        var url =
            $"{BaseUrl}/track/{Uri.EscapeDataString(trackId)}";

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        try
        {
            using var document =
                JsonDocument.Parse(json);

            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // Audio Features
    // ============================================================

    private async Task<JsonElement?>
        GetAudioFeaturesAsync(
            string trackId,
            CancellationToken cancellationToken)
    {
        var url =
            $"{BaseUrl}/track/" +
            $"{Uri.EscapeDataString(trackId)}" +
            "/audio-features";

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        try
        {
            using var document =
                JsonDocument.Parse(json);

            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // Artist
    // ============================================================

    private static string GetArtistNames(
        JsonElement? details,
        JsonElement searchResult)
    {
        if (details.HasValue &&
            details.Value.TryGetProperty(
                "artists",
                out var detailArtists) &&
            detailArtists.ValueKind ==
                JsonValueKind.Array)
        {
            var names =
                detailArtists
                    .EnumerateArray()
                    .Select(
                        artist =>
                            GetString(
                                artist,
                                "name"))
                    .Where(
                        name =>
                            !string.IsNullOrWhiteSpace(name))
                    .ToList();

            if (names.Count > 0)
            {
                return string.Join(
                    ", ",
                    names);
            }
        }

        if (searchResult.TryGetProperty(
                "artists",
                out var searchArtists) &&
            searchArtists.ValueKind ==
                JsonValueKind.Array)
        {
            var names =
                searchArtists
                    .EnumerateArray()
                    .Select(
                        artist =>
                            GetString(
                                artist,
                                "name"))
                    .Where(
                        name =>
                            !string.IsNullOrWhiteSpace(name))
                    .ToList();

            if (names.Count > 0)
            {
                return string.Join(
                    ", ",
                    names);
            }
        }

        return string.Empty;
    }

    // ============================================================
    // Duration
    // ============================================================

    private static TimeSpan? GetDuration(
        JsonElement? details,
        JsonElement searchResult)
    {
        var durationMs =
            GetNullableDouble(
                details,
                "durationMs");

        if (!durationMs.HasValue)
        {
            durationMs =
                GetNullableDouble(
                    searchResult,
                    "durationMs");
        }

        if (!durationMs.HasValue ||
            durationMs.Value <= 0)
        {
            return null;
        }

        return TimeSpan.FromMilliseconds(
            durationMs.Value);
    }

    // ============================================================
    // Musical Key
    // ============================================================

    /// <summary>
    /// Converts the Spotify/ReccoBeats numeric key and mode
    /// representation into a conventional musical key.
    ///
    /// key:
    /// 0 = C
    /// 1 = C#
    /// 2 = D
    /// 3 = Eb
    /// 4 = E
    /// 5 = F
    /// 6 = F#
    /// 7 = G
    /// 8 = Ab
    /// 9 = A
    /// 10 = Bb
    /// 11 = B
    ///
    /// mode:
    /// 0 = minor
    /// 1 = major
    /// </summary>
    private static string BuildMusicalKey(
        JsonElement? features)
    {
        if (!features.HasValue)
        {
            return string.Empty;
        }

        var key =
            GetNullableInt(
                features,
                "key");

        var mode =
            GetNullableInt(
                features,
                "mode");

        if (!key.HasValue ||
            !mode.HasValue)
        {
            return string.Empty;
        }

        if (key.Value < 0 ||
            key.Value > 11)
        {
            return string.Empty;
        }

        if (mode.Value != 0 &&
            mode.Value != 1)
        {
            return string.Empty;
        }

        string[] majorKeys =
        {
            "C",
            "C#",
            "D",
            "Eb",
            "E",
            "F",
            "F#",
            "G",
            "Ab",
            "A",
            "Bb",
            "B"
        };

        string[] minorKeys =
        {
            "Cm",
            "C#m",
            "Dm",
            "Ebm",
            "Em",
            "Fm",
            "F#m",
            "Gm",
            "Abm",
            "Am",
            "Bbm",
            "Bm"
        };

        return mode.Value == 1
            ? majorKeys[key.Value]
            : minorKeys[key.Value];
    }

    // ============================================================
    // Confidence
    // ============================================================

    private static double CalculateConfidence(
        MetadataSearchRequest request,
        string artist,
        string title,
        TimeSpan? duration)
    {
        double score = 0;

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            !string.IsNullOrWhiteSpace(
                artist))
        {
            if (Normalise(request.Artist)
                .Equals(
                    Normalise(artist),
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 45;
            }
            else if (
                Normalise(artist)
                    .Contains(
                        Normalise(request.Artist),
                        StringComparison.OrdinalIgnoreCase) ||
                Normalise(request.Artist)
                    .Contains(
                        Normalise(artist),
                        StringComparison.OrdinalIgnoreCase))
            {
                score += 25;
            }
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title) &&
            !string.IsNullOrWhiteSpace(
                title))
        {
            if (NormaliseTitle(request.Title)
                .Equals(
                    NormaliseTitle(title),
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 45;
            }
            else if (
                NormaliseTitle(title)
                    .Contains(
                        NormaliseTitle(request.Title),
                        StringComparison.OrdinalIgnoreCase) ||
                NormaliseTitle(request.Title)
                    .Contains(
                        NormaliseTitle(title),
                        StringComparison.OrdinalIgnoreCase))
            {
                score += 25;
            }
        }

        if (request.Duration.HasValue &&
            duration.HasValue)
        {
            var difference =
                Math.Abs(
                    (
                        request.Duration.Value -
                        duration.Value)
                    .TotalSeconds);

            if (difference <= 2)
            {
                score += 10;
            }
            else if (difference <= 5)
            {
                score += 7;
            }
            else if (difference <= 10)
            {
                score += 4;
            }
        }

        return Math.Round(
            Math.Clamp(
                score,
                0,
                100),
            1);
    }

    // ============================================================
    // Match Reason
    // ============================================================

    private static string BuildMatchReason(
        MetadataSearchRequest request,
        string artist,
        string title,
        TimeSpan? duration,
        double? bpm,
        string key)
    {
        var reasons =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            Normalise(request.Artist)
                .Equals(
                    Normalise(artist),
                    StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(
                "Artist matches");
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title) &&
            NormaliseTitle(request.Title)
                .Equals(
                    NormaliseTitle(title),
                    StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(
                "Title matches");
        }

        if (request.Duration.HasValue &&
            duration.HasValue)
        {
            var difference =
                Math.Abs(
                    (
                        request.Duration.Value -
                        duration.Value)
                    .TotalSeconds);

            if (difference <= 2)
            {
                reasons.Add(
                    "Duration closely matches");
            }
            else if (difference <= 10)
            {
                reasons.Add(
                    "Duration is similar");
            }
        }

        if (bpm.HasValue)
        {
            reasons.Add(
                $"ReccoBeats BPM {bpm.Value:0.###}");
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            reasons.Add(
                $"ReccoBeats key {key}");
        }

        if (reasons.Count == 0)
        {
            return "ReccoBeats candidate";
        }

        return string.Join(
            "; ",
            reasons);
    }

    // ============================================================
    // JSON Helpers
    // ============================================================

    private static string? GetString(
        JsonElement? element,
        string propertyName)
    {
        if (!element.HasValue)
        {
            return null;
        }

        return GetString(
            element.Value,
            propertyName);
    }

    private static string? GetString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static double? GetNullableDouble(
        JsonElement? element,
        string propertyName)
    {
        if (!element.HasValue)
        {
            return null;
        }

        return GetNullableDouble(
            element.Value,
            propertyName);
    }

    private static double? GetNullableDouble(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Number &&
            property.TryGetDouble(
                out var number))
        {
            return number;
        }

        if (property.ValueKind ==
                JsonValueKind.String &&
            double.TryParse(
                property.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number))
        {
            return number;
        }

        return null;
    }

    private static int? GetNullableInt(
        JsonElement? element,
        string propertyName)
    {
        if (!element.HasValue)
        {
            return null;
        }

        return GetNullableInt(
            element.Value,
            propertyName);
    }

    private static int? GetNullableInt(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Number &&
            property.TryGetInt32(
                out var number))
        {
            return number;
        }

        if (property.ValueKind ==
                JsonValueKind.String &&
            int.TryParse(
                property.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out number))
        {
            return number;
        }

        return null;
    }

    // ============================================================
    // Normalisation
    // ============================================================

    private static string Normalise(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .ToUpperInvariant();
    }

    private static string NormaliseTitle(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .ToUpperInvariant()
            .Replace(
                "(FEAT.",
                "FEAT.",
                StringComparison.Ordinal)
            .Replace(
                "(FEAT ",
                "FEAT ",
                StringComparison.Ordinal);
    }
}