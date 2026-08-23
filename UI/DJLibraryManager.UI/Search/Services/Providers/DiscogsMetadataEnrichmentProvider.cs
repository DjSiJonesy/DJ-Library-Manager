using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Performs second-stage metadata discovery against Discogs.
///
/// This provider is only used after the primary recording search
/// has established the Artist/Title identity.
///
/// It does not attempt to identify a different recording.
///
/// Discogs release metadata can provide Genre and ReleaseYear.
/// ReleaseYear is deliberately kept separate from Year because a
/// release year does not necessarily represent the original
/// recording year.
/// </summary>
public sealed class DiscogsMetadataEnrichmentProvider
    : IMetadataEnrichmentProvider
{
    private const string SearchUrl =
        "https://api.discogs.com/database/search";

    private const string ReleaseUrl =
        "https://api.discogs.com/releases/";

    private const string UserAgent =
        "DIASISS/0.1.0 (DJ Library Manager)";

    private const int SearchResultLimit = 10;

    private const int MaximumCandidatesToInspect = 10;

    private static readonly HttpClient HttpClient =
        CreateHttpClient();

    public string Name =>
        "Discogs";

    // ============================================================
    // Enrich
    // ============================================================

    public async Task<
        IReadOnlyList<MetadataSearchProviderResult>>
        EnrichAsync(
            MetadataEnrichmentRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (request.MissingFields is null ||
            request.MissingFields.Count == 0)
        {
            return [];
        }

        var wantsGenre =
            request.MissingFields.Any(
                field =>
                    field.Equals(
                        "Genre",
                        StringComparison.OrdinalIgnoreCase));

        var wantsYear =
            request.MissingFields.Any(
                field =>
                    field.Equals(
                        "Year",
                        StringComparison.OrdinalIgnoreCase));

        //
        // Discogs cannot safely turn release.year into the
        // recording Year field.
        //
        // We can still perform the search when Year is requested,
        // because the result may contain Genre and ReleaseYear.
        //

        if (!wantsGenre &&
            !wantsYear)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(request.Artist) &&
            string.IsNullOrWhiteSpace(request.Title))
        {
            return [];
        }

        var token =
            Environment.GetEnvironmentVariable(
                "DISCOGS_API_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        var searchResults =
            await SearchReleasesAsync(
                request,
                token,
                cancellationToken);

        if (searchResults.Count == 0)
        {
            return [];
        }

        var results =
            new List<MetadataSearchProviderResult>();

        var inspected =
            0;

        foreach (var searchResult in searchResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (inspected >= MaximumCandidatesToInspect)
            {
                break;
            }

            var releaseId =
                GetString(
                    searchResult,
                    "id");

            if (string.IsNullOrWhiteSpace(releaseId))
            {
                continue;
            }

            inspected++;

            var release =
                await GetReleaseAsync(
                    releaseId,
                    token,
                    cancellationToken);

            if (!release.HasValue)
            {
                continue;
            }

            var releaseValue =
                release.Value;

            var matchingTrack =
                FindMatchingTrack(
                    releaseValue,
                    request);

            if (!matchingTrack.HasValue)
            {
                continue;
            }

            var result =
                CreateEnrichmentResult(
                    releaseValue,
                    matchingTrack.Value,
                    request);

            if (result is not null)
            {
                results.Add(
                    result);
            }
        }

        return results
            .OrderByDescending(
                result =>
                    result.Confidence)
            .Take(SearchResultLimit)
            .ToList();
    }

    // ============================================================
    // Release Search
    // ============================================================

    private static async Task<
        List<JsonElement>>
        SearchReleasesAsync(
            MetadataEnrichmentRequest request,
            string token,
            CancellationToken cancellationToken)
    {
        var parameters =
            new List<string>
            {
                "type=release",
                $"per_page={SearchResultLimit}",
                "page=1"
            };

        var artist =
            CleanSearchText(
                request.Artist);

        var title =
            CleanSearchText(
                request.Title);

        if (!string.IsNullOrWhiteSpace(artist))
        {
            parameters.Add(
                $"artist={Uri.EscapeDataString(artist)}");
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            parameters.Add(
                $"track={Uri.EscapeDataString(title)}");
        }

        if (string.IsNullOrWhiteSpace(artist) ||
            string.IsNullOrWhiteSpace(title))
        {
            var query =
                !string.IsNullOrWhiteSpace(title)
                    ? title
                    : artist;

            if (!string.IsNullOrWhiteSpace(query))
            {
                parameters.Add(
                    $"q={Uri.EscapeDataString(query)}");
            }
        }

        var url =
            $"{SearchUrl}?{string.Join(
                "&",
                parameters)}";

        using var requestMessage =
            CreateRequest(
                url,
                token);

        using var response =
            await HttpClient.SendAsync(
                requestMessage,
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
                    "results",
                    out var results) ||
                results.ValueKind !=
                    JsonValueKind.Array)
            {
                return [];
            }

            return results
                .EnumerateArray()
                .Select(
                    result =>
                        result.Clone())
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    // ============================================================
    // Retrieve Release
    // ============================================================

    private static async Task<JsonElement?>
        GetReleaseAsync(
            string releaseId,
            string token,
            CancellationToken cancellationToken)
    {
        var url =
            $"{ReleaseUrl}" +
            Uri.EscapeDataString(
                releaseId);

        using var requestMessage =
            CreateRequest(
                url,
                token);

        using var response =
            await HttpClient.SendAsync(
                requestMessage,
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
    // Find Matching Track
    // ============================================================

    private static JsonElement?
        FindMatchingTrack(
            JsonElement release,
            MetadataEnrichmentRequest request)
    {
        if (!release.TryGetProperty(
                "tracklist",
                out var tracklist) ||
            tracklist.ValueKind !=
                JsonValueKind.Array)
        {
            return null;
        }

        JsonElement?
            bestTrack = null;

        var bestScore =
            0d;

        foreach (var track in
                 tracklist.EnumerateArray())
        {
            var type =
                GetString(
                    track,
                    "type_");

            if (string.Equals(
                    type,
                    "heading",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var title =
                GetString(
                    track,
                    "title");

            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var artist =
                GetTrackArtist(
                    track,
                    release);

            var score =
                CalculateIdentityScore(
                    request,
                    artist,
                    title);

            if (score > bestScore)
            {
                bestScore =
                    score;

                bestTrack =
                    track.Clone();
            }
        }

        //
        // Enrichment must still be tied to the established
        // recording identity.
        //

        return bestScore >= 60
            ? bestTrack
            : null;
    }

    // ============================================================
    // Identity Score
    // ============================================================

    private static double
        CalculateIdentityScore(
            MetadataEnrichmentRequest request,
            string artist,
            string title)
    {
        var score =
            0d;

        if (!string.IsNullOrWhiteSpace(
                request.Artist))
        {
            if (!ArtistMatches(
                    request.Artist,
                    artist))
            {
                return 0;
            }

            score += 50;
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title))
        {
            var titleScore =
                CalculateTitleScore(
                    request.Title,
                    title);

            score +=
                titleScore;
        }

        return score;
    }

    // ============================================================
    // Create Enrichment Result
    // ============================================================

    private static MetadataSearchProviderResult?
        CreateEnrichmentResult(
            JsonElement release,
            JsonElement track,
            MetadataEnrichmentRequest request)
    {
        var trackTitle =
            GetString(
                track,
                "title");

        if (string.IsNullOrWhiteSpace(
                trackTitle))
        {
            return null;
        }

        var trackArtist =
            GetTrackArtist(
                track,
                release);

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            !ArtistMatches(
                request.Artist,
                trackArtist))
        {
            return null;
        }

        var titleScore =
            CalculateTitleScore(
                request.Title,
                trackTitle);

        if (!string.IsNullOrWhiteSpace(
                request.Title) &&
            titleScore < 30)
        {
            return null;
        }

        var releaseId =
            GetString(
                release,
                "id");

        var releaseTitle =
            GetString(
                release,
                "title");

        var releaseYear =
            GetInt(
                release,
                "year");

        var genre =
            GetFirstArrayValue(
                release,
                "genres");

        var style =
            GetFirstArrayValue(
                release,
                "styles");

        var duration =
            ParseDuration(
                GetString(
                    track,
                    "duration"));

        var confidence =
            CalculateConfidence(
                request,
                trackArtist,
                trackTitle,
                genre,
                style,
                releaseYear,
                duration,
                titleScore);

        return new MetadataSearchProviderResult
        {
            Source =
                "Discogs Enrichment",

            ExternalId =
                releaseId,

            Artist =
                trackArtist,

            Title =
                trackTitle,

            Album =
                releaseTitle,

            Genre =
                BuildGenre(
                    genre,
                    style),

            //
            // IMPORTANT:
            //
            // Discogs release.year is NOT promoted to Year.
            //

            Year =
                null,

            ReleaseYear =
                releaseYear,

            BPM =
                null,

            Key =
                string.Empty,

            Duration =
                duration,

            Confidence =
                confidence,

            MatchReason =
                BuildMatchReason(
                    request,
                    trackArtist,
                    trackTitle,
                    genre,
                    style,
                    releaseYear,
                    releaseId)
        };
    }

    // ============================================================
    // Confidence
    // ============================================================

    private static double
        CalculateConfidence(
            MetadataEnrichmentRequest request,
            string artist,
            string title,
            string genre,
            string style,
            int? releaseYear,
            TimeSpan? duration,
            double titleScore)
    {
        var score =
            0d;

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            ArtistMatches(
                request.Artist,
                artist))
        {
            score += 40;
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title))
        {
            score +=
                titleScore * 0.5;
        }

        if (!string.IsNullOrWhiteSpace(
                genre))
        {
            score += 5;
        }

        if (!string.IsNullOrWhiteSpace(
                style))
        {
            score += 5;
        }

        if (releaseYear.HasValue)
        {
            score += 5;
        }

        if (request.Duration.HasValue &&
            duration.HasValue)
        {
            var difference =
                Math.Abs(
                    (
                        request.Duration.Value -
                        duration.Value
                    ).TotalSeconds);

            if (difference <= 2)
            {
                score += 10;
            }
            else if (difference <= 5)
            {
                score += 5;
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
        MetadataEnrichmentRequest request,
        string artist,
        string title,
        string genre,
        string style,
        int? releaseYear,
        string releaseId)
    {
        var parts =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            ArtistMatches(
                request.Artist,
                artist))
        {
            parts.Add(
                "Established Artist identity matched");
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title) &&
            CalculateTitleScore(
                request.Title,
                title) >= 45)
        {
            parts.Add(
                "Established Title identity matched");
        }

        if (!string.IsNullOrWhiteSpace(
                genre))
        {
            parts.Add(
                $"Genre: {genre}");
        }

        if (!string.IsNullOrWhiteSpace(
                style))
        {
            parts.Add(
                $"Style: {style}");
        }

        if (releaseYear.HasValue)
        {
            parts.Add(
                $"Release year: {releaseYear.Value}");
        }

        if (!string.IsNullOrWhiteSpace(
                releaseId))
        {
            parts.Add(
                $"Discogs release ID: {releaseId}");
        }

        return string.Join(
            "; ",
            parts);
    }

    // ============================================================
    // Track Artist
    // ============================================================

    private static string GetTrackArtist(
        JsonElement track,
        JsonElement release)
    {
        if (track.TryGetProperty(
                "artists",
                out var artists) &&
            artists.ValueKind ==
                JsonValueKind.Array)
        {
            var names =
                new List<string>();

            foreach (var artist in
                     artists.EnumerateArray())
            {
                var name =
                    GetString(
                        artist,
                        "name");

                if (!string.IsNullOrWhiteSpace(
                        name))
                {
                    names.Add(
                        name.Trim());
                }
            }

            if (names.Count > 0)
            {
                return string.Join(
                    ", ",
                    names);
            }
        }

        if (release.TryGetProperty(
                "artists",
                out var releaseArtists) &&
            releaseArtists.ValueKind ==
                JsonValueKind.Array)
        {
            var names =
                new List<string>();

            foreach (var artist in
                     releaseArtists.EnumerateArray())
            {
                var name =
                    GetString(
                        artist,
                        "name");

                if (!string.IsNullOrWhiteSpace(
                        name))
                {
                    names.Add(
                        name.Trim());
                }
            }

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
    // Artist Matching
    // ============================================================

    private static bool ArtistMatches(
        string? requested,
        string? returned)
    {
        var requestedNormalised =
            Normalise(
                requested);

        var returnedNormalised =
            Normalise(
                returned);

        if (string.IsNullOrWhiteSpace(
                requestedNormalised) ||
            string.IsNullOrWhiteSpace(
                returnedNormalised))
        {
            return false;
        }

        if (string.Equals(
                requestedNormalised,
                returnedNormalised,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var requestedTokens =
            requestedNormalised
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

        var returnedTokens =
            returnedNormalised
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

        return requestedTokens.Length > 0 &&
               requestedTokens.All(
                   token =>
                       returnedTokens.Contains(
                           token,
                           StringComparer.OrdinalIgnoreCase));
    }

    // ============================================================
    // Title Matching
    // ============================================================

    private static double CalculateTitleScore(
        string? requested,
        string? returned)
    {
        if (string.IsNullOrWhiteSpace(requested) ||
            string.IsNullOrWhiteSpace(returned))
        {
            return 0;
        }

        var requestedNormalised =
            NormaliseTitle(
                requested);

        var returnedNormalised =
            NormaliseTitle(
                returned);

        if (string.Equals(
                requestedNormalised,
                returnedNormalised,
                StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        var requestedBase =
            NormaliseTitle(
                StripVersionInformation(
                    requested));

        var returnedBase =
            NormaliseTitle(
                StripVersionInformation(
                    returned));

        if (string.Equals(
                requestedBase,
                returnedBase,
                StringComparison.OrdinalIgnoreCase))
        {
            return 45;
        }

        if (requestedBase.Length >= 6 &&
            returnedBase.Length >= 6 &&
            (
                requestedBase.Contains(
                    returnedBase,
                    StringComparison.OrdinalIgnoreCase)
                ||
                returnedBase.Contains(
                    requestedBase,
                    StringComparison.OrdinalIgnoreCase)
            ))
        {
            return 30;
        }

        return 0;
    }

    // ============================================================
    // Title Normalisation
    // ============================================================

    private static string NormaliseTitle(
        string? value)
    {
        return Normalise(
            value);
    }

    private static string StripVersionInformation(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        var result =
            value.Trim();

        result =
            System.Text.RegularExpressions.Regex.Replace(
                result,
                @"\s+\d{1,2}\s*(?:""|" +
                "″" +
                @"|inch(?:es)?|in)\s*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        result =
            System.Text.RegularExpressions.Regex.Replace(
                result,
                @"\s*\([^()]*\)\s*$",
                string.Empty);

        result =
            System.Text.RegularExpressions.Regex.Replace(
                result,
                @"\s+-\s+(?:remix|mix|edit|version|radio edit|"
                + @"extended|extended mix|club mix|"
                + @"instrumental|acoustic).*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return result.Trim();
    }

    private static string Normalise(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        var builder =
            new StringBuilder();

        foreach (var character in
                 value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(
                    character))
            {
                builder.Append(
                    character);
            }
            else
            {
                builder.Append(
                    ' ');
            }
        }

        return string.Join(
            " ",
            builder
                .ToString()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }

    // ============================================================
    // Genre
    // ============================================================

    private static string BuildGenre(
        string genre,
        string style)
    {
        if (!string.IsNullOrWhiteSpace(
                genre) &&
            !string.IsNullOrWhiteSpace(
                style))
        {
            return $"{genre} / {style}";
        }

        return
            !string.IsNullOrWhiteSpace(
                genre)
                ? genre
                : style;
    }

    // ============================================================
    // Duration
    // ============================================================

    private static TimeSpan? ParseDuration(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts =
            value.Trim().Split(
                ':',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2 &&
            int.TryParse(
                parts[0],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var minutes) &&
            int.TryParse(
                parts[1],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var seconds))
        {
            if (minutes >= 0 &&
                seconds >= 0 &&
                seconds < 60)
            {
                return new TimeSpan(
                    0,
                    minutes,
                    seconds);
            }
        }

        if (parts.Length == 3 &&
            int.TryParse(
                parts[0],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var hours) &&
            int.TryParse(
                parts[1],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out minutes) &&
            int.TryParse(
                parts[2],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out seconds))
        {
            if (hours >= 0 &&
                minutes >= 0 &&
                minutes < 60 &&
                seconds >= 0 &&
                seconds < 60)
            {
                return new TimeSpan(
                    hours,
                    minutes,
                    seconds);
            }
        }

        if (TimeSpan.TryParse(
                value.Trim(),
                CultureInfo.InvariantCulture,
                out var duration))
        {
            return duration;
        }

        return null;
    }

    // ============================================================
    // JSON Helpers
    // ============================================================

    private static string GetString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return string.Empty;
        }

        if (property.ValueKind ==
            JsonValueKind.String)
        {
            return property.GetString() ??
                   string.Empty;
        }

        if (property.ValueKind ==
            JsonValueKind.Number)
        {
            return property.GetRawText();
        }

        return string.Empty;
    }

    private static int? GetInt(
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
                out var value))
        {
            return value;
        }

        if (property.ValueKind ==
                JsonValueKind.String &&
            int.TryParse(
                property.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value))
        {
            return value;
        }

        return null;
    }

    private static string GetFirstArrayValue(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property) ||
            property.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var item in
                 property.EnumerateArray())
        {
            if (item.ValueKind ==
                JsonValueKind.String)
            {
                var value =
                    item.GetString();

                if (!string.IsNullOrWhiteSpace(
                        value))
                {
                    return value;
                }
            }
        }

        return string.Empty;
    }

    // ============================================================
    // HTTP
    // ============================================================

    private static HttpRequestMessage CreateRequest(
        string url,
        string token)
    {
        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Discogs",
                $"token={token}");

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        return request;
    }

    private static HttpClient CreateHttpClient()
    {
        var client =
            new HttpClient();

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            UserAgent);

        client.DefaultRequestHeaders.Accept.ParseAdd(
            "application/json");

        return client;
    }

    // ============================================================
    // Utility
    // ============================================================

    private static string CleanSearchText(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .Replace(
                "\"",
                string.Empty)
            .Replace(
                "″",
                string.Empty);
    }
}