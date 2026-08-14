using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Searches MusicBrainz for possible recording metadata.
///
/// MusicBrainz is a discovery source only. This provider does not
/// modify the DIASISS library or physical media files.
///
/// Search deliberately uses broad queries. DIASISS performs the
/// final candidate matching and confidence evaluation.
/// </summary>
public sealed class MusicBrainzMetadataProvider
    : IMetadataSearchProvider
{
    private const string ApiBaseUrl =
        "https://musicbrainz.org/ws/2/recording";

    private const string UserAgent =
        "DIASISS/0.1.0 (DJ Library Manager)";

    private const int ResultLimit = 10;

    private static readonly HttpClient HttpClient =
        CreateHttpClient();

    private static readonly SemaphoreSlim RateLimit =
        new(1, 1);

    private static DateTime _lastRequestUtc =
        DateTime.MinValue;

    public string Name =>
        "MusicBrainz";

    // ============================================================
    // Search
    // ============================================================

    public async Task<
        IReadOnlyList<MetadataSearchProviderResult>>
        SearchAsync(
            MetadataSearchRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Artist) &&
            string.IsNullOrWhiteSpace(request.Title))
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "No Artist or Title was supplied.")
            ];
        }

        var queries =
            BuildSearchQueries(request);

        var allResults =
            new List<MetadataSearchProviderResult>();

        var seenIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var query in queries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var results =
                await ExecuteQueryAsync(
                    query,
                    request,
                    cancellationToken);

            foreach (var result in results)
            {
                if (!string.IsNullOrWhiteSpace(
                        result.ExternalId))
                {
                    if (!seenIds.Add(
                            result.ExternalId))
                    {
                        continue;
                    }
                }

                allResults.Add(result);
            }

            // ----------------------------------------------------
            // Once we have usable candidates, do not unnecessarily
            // hammer MusicBrainz with additional searches.
            // ----------------------------------------------------

            if (allResults.Count >= ResultLimit)
            {
                break;
            }
        }

        if (allResults.Count == 0)
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "MusicBrainz returned HTTP 200 but " +
                    "no usable recordings were found.")
            ];
        }

        return
            allResults
                .OrderByDescending(
                    result => result.Confidence)
                .ThenBy(
                    result => result.Artist,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    result => result.Title,
                    StringComparer.OrdinalIgnoreCase)
                .Take(ResultLimit)
                .ToList();
    }

    // ============================================================
    // Build Search Queries
    // ============================================================

    private static IReadOnlyList<string>
        BuildSearchQueries(
            MetadataSearchRequest request)
    {
        var queries =
            new List<string>();

        var title =
            CleanTitleForSearch(
                request.Title);

        var artist =
            CleanArtistForSearch(
                request.Artist);

        // --------------------------------------------------------
        // Preferred search:
        //
        // Search by core title only.
        //
        // This deliberately avoids requiring MusicBrainz to
        // understand DJ-specific suffixes such as:
        //
        // (Remix)
        // (12")
        // (Radio Edit)
        // (Official Video)
        // (720p60fps)
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(title))
        {
            queries.Add(
                $"recording:\"{EscapeLucene(title)}\"");
        }

        // --------------------------------------------------------
        // Secondary search:
        //
        // Artist + core title.
        //
        // This provides additional precision without making the
        // first query dependent on exact artist formatting.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(artist) &&
            !string.IsNullOrWhiteSpace(title))
        {
            queries.Add(
                $"artist:\"{EscapeLucene(artist)}\" " +
                $"AND recording:\"{EscapeLucene(title)}\"");
        }

        // --------------------------------------------------------
        // Artist-only fallback.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(artist))
        {
            queries.Add(
                $"artist:\"{EscapeLucene(artist)}\"");
        }

        return queries
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // Execute Query
    // ============================================================

    private static async Task<
        List<MetadataSearchProviderResult>>
        ExecuteQueryAsync(
            string query,
            MetadataSearchRequest request,
            CancellationToken cancellationToken)
    {
        var url =
            $"{ApiBaseUrl}" +
            $"?query={Uri.EscapeDataString(query)}" +
            $"&limit={ResultLimit}" +
            "&inc=artist-credits+releases" +
            "&fmt=json";

        await WaitForRateLimitAsync(
            cancellationToken);

        using var response =
            await HttpClient.GetAsync(
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

            return ParseResults(
                document.RootElement,
                request);
        }
        catch
        {
            return [];
        }
    }

    // ============================================================
    // Parse Results
    // ============================================================

    private static List<
        MetadataSearchProviderResult>
        ParseResults(
            JsonElement root,
            MetadataSearchRequest request)
    {
        if (!root.TryGetProperty(
                "recordings",
                out var recordings) ||
            recordings.ValueKind !=
                JsonValueKind.Array)
        {
            return [];
        }

        var results =
            new List<MetadataSearchProviderResult>();

        foreach (var recording in
                 recordings.EnumerateArray())
        {
            var result =
                ParseRecording(
                    recording,
                    request);

            if (result is not null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    // ============================================================
    // Parse Recording
    // ============================================================

    private static MetadataSearchProviderResult?
        ParseRecording(
            JsonElement recording,
            MetadataSearchRequest request)
    {
        var title =
            GetString(
                recording,
                "title");

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var artist =
            GetArtistCredit(
                recording);

        var album =
            GetFirstReleaseTitle(
                recording);

        var year =
            GetFirstReleaseYear(
                recording);

        var duration =
            GetDuration(
                recording);

        var musicBrainzScore =
            GetDouble(
                recording,
                "score");

        var recordingId =
            GetString(
                recording,
                "id");

        var confidence =
            CalculateConfidence(
                musicBrainzScore,
                request,
                artist,
                title,
                duration);

        return new MetadataSearchProviderResult
        {
            Source =
                "MusicBrainz",

            ExternalId =
                recordingId,

            Artist =
                artist,

            Title =
                title,

            Album =
                album,

            Year =
                year,

            Duration =
                duration,

            Confidence =
                confidence,

            MatchReason =
                BuildMatchReason(
                    request,
                    artist,
                    title,
                    album,
                    year,
                    duration,
                    confidence)
        };
    }

    // ============================================================
    // Artist Credit
    // ============================================================

    private static string GetArtistCredit(
        JsonElement recording)
    {
        if (!recording.TryGetProperty(
                "artist-credit",
                out var credits) ||
            credits.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        var artists =
            new List<string>();

        foreach (var credit in
                 credits.EnumerateArray())
        {
            var name =
                GetString(
                    credit,
                    "name");

            if (!string.IsNullOrWhiteSpace(name))
            {
                artists.Add(name);
            }
        }

        return string.Join(
            ", ",
            artists);
    }

    // ============================================================
    // Album
    // ============================================================

    private static string GetFirstReleaseTitle(
        JsonElement recording)
    {
        if (!recording.TryGetProperty(
                "release-list",
                out var releases) ||
            releases.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        var first =
            releases
                .EnumerateArray()
                .FirstOrDefault();

        if (first.ValueKind ==
            JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return GetString(
            first,
            "title");
    }

    // ============================================================
    // Year
    // ============================================================

    private static int? GetFirstReleaseYear(
        JsonElement recording)
    {
        var date =
            GetString(
                recording,
                "first-release-date");

        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        if (date.Length >= 4 &&
            int.TryParse(
                date[..4],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var year))
        {
            return year;
        }

        return null;
    }

    // ============================================================
    // Duration
    // ============================================================

    private static TimeSpan? GetDuration(
        JsonElement recording)
    {
        if (!recording.TryGetProperty(
                "length",
                out var length))
        {
            return null;
        }

        if (length.ValueKind !=
            JsonValueKind.Number)
        {
            return null;
        }

        if (!length.TryGetInt64(
                out var milliseconds))
        {
            return null;
        }

        if (milliseconds <= 0)
        {
            return null;
        }

        return TimeSpan.FromMilliseconds(
            milliseconds);
    }

    // ============================================================
    // Confidence
    // ============================================================

    private static double CalculateConfidence(
        double musicBrainzScore,
        MetadataSearchRequest request,
        string artist,
        string title,
        TimeSpan? duration)
    {
        var confidence =
            musicBrainzScore;

        if (ArtistMatches(
                request.Artist,
                artist))
        {
            confidence += 10;
        }

        if (TitleMatches(
                request.Title,
                title))
        {
            confidence += 10;
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
                confidence += 5;
            }
            else if (difference <= 5)
            {
                confidence += 3;
            }
            else if (difference <= 10)
            {
                confidence += 1;
            }
        }

        return Math.Round(
            Math.Clamp(
                confidence,
                0,
                100),
            1);
    }

    // ============================================================
    // Artist Matching
    // ============================================================

    private static bool ArtistMatches(
        string? requested,
        string? returned)
    {
        var requestedTokens =
            Tokenise(requested);

        var returnedTokens =
            Tokenise(returned);

        if (requestedTokens.Count == 0 ||
            returnedTokens.Count == 0)
        {
            return false;
        }

        return requestedTokens
            .All(
                token =>
                    returnedTokens.Contains(
                        token,
                        StringComparer.OrdinalIgnoreCase));
    }

    // ============================================================
    // Title Matching
    // ============================================================

    private static bool TitleMatches(
        string? requested,
        string? returned)
    {
        var requestedTitle =
            CleanTitleForSearch(
                requested);

        var returnedTitle =
            CleanTitleForSearch(
                returned);

        if (string.IsNullOrWhiteSpace(
                requestedTitle) ||
            string.IsNullOrWhiteSpace(
                returnedTitle))
        {
            return false;
        }

        return string.Equals(
                   Normalise(requestedTitle),
                   Normalise(returnedTitle),
                   StringComparison.OrdinalIgnoreCase)
               ||
               Normalise(requestedTitle)
                   .Contains(
                       Normalise(returnedTitle),
                       StringComparison.OrdinalIgnoreCase)
               ||
               Normalise(returnedTitle)
                   .Contains(
                       Normalise(requestedTitle),
                       StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // Match Explanation
    // ============================================================

    private static string BuildMatchReason(
        MetadataSearchRequest request,
        string artist,
        string title,
        string album,
        int? year,
        TimeSpan? duration,
        double confidence)
    {
        var parts =
            new List<string>();

        if (ArtistMatches(
                request.Artist,
                artist))
        {
            parts.Add(
                "Artist matched");
        }

        if (TitleMatches(
                request.Title,
                title))
        {
            parts.Add(
                "Title matched");
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
                parts.Add(
                    "Duration matched");
            }
            else if (difference <= 10)
            {
                parts.Add(
                    $"Duration within {difference:F0}s");
            }
        }

        if (!string.IsNullOrWhiteSpace(album))
        {
            parts.Add(
                $"Album found: {album}");
        }

        if (year.HasValue)
        {
            parts.Add(
                $"Year found: {year.Value}");
        }

        parts.Add(
            $"Confidence: {confidence:F0}%");

        return string.Join(
            "; ",
            parts);
    }

    // ============================================================
    // Filename / Title Cleaning
    // ============================================================

    private static string CleanTitleForSearch(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var title =
            value.Trim();

        // Remove file extension if one has accidentally
        // reached the provider.
        var lastDot =
            title.LastIndexOf('.');

        if (lastDot > 0 &&
            lastDot >= title.Length - 5)
        {
            title =
                title[..lastDot];
        }

        // Remove common video/technical suffixes.
        title =
            System.Text.RegularExpressions.Regex.Replace(
                title,
                @"\s*\((?:\d{3,4}p|\d{2,3}0p\d*fps)\)\s*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        title =
            System.Text.RegularExpressions.Regex.Replace(
                title,
                @"\s*\((?:official\s+video|lyrics?|lyric\s+video)\)\s*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Remove trailing format/version notation such as:
        //
        // 12"
        // 7"
        // 12 inch
        // 12-inch
        //
        title =
    System.Text.RegularExpressions.Regex.Replace(
        title,
        "\\s+(?:\\d{1,2}(?:\\.\\d+)?[\"″]|\\d{1,2}\\s*(?:inch|in))\\s*$",
        string.Empty,
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Remove trailing technical parenthetical information,
        // but retain the main title before it.
        //
        // Example:
        //
        // Never Too Much ('89 Remix)
        //
        // becomes:
        //
        // Never Too Much
        //
        title =
            System.Text.RegularExpressions.Regex.Replace(
                title,
                @"\s*\([^()]*\)\s*$",
                string.Empty);

        // Remove common separators left at the end.
        title =
            title
                .Trim()
                .Trim(
                    '-',
                    '_',
                    '–',
                    '—');

        return title.Trim();
    }

    private static string CleanArtistForSearch(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .Trim(
                '-',
                '_',
                '–',
                '—');
    }

    // ============================================================
    // Tokenisation
    // ============================================================

    private static List<string> Tokenise(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return
            Normalise(value)
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)
                .ToList();
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

        var builder =
            new StringBuilder();

        foreach (var character in
                 value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else
            {
                builder.Append(' ');
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

        return property.ValueKind ==
               JsonValueKind.String
            ? property.GetString() ??
              string.Empty
            : string.Empty;
    }

    private static double GetDouble(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return 0;
        }

        if (property.ValueKind ==
                JsonValueKind.Number &&
            property.TryGetDouble(
                out var number))
        {
            return number;
        }

        return 0;
    }

    // ============================================================
    // Lucene Escaping
    // ============================================================

    private static string EscapeLucene(
        string value)
    {
        const string specialCharacters =
            "\\+-&|!(){}[]^\"~*?:/";

        var result =
            new StringBuilder();

        foreach (var character in value)
        {
            if (specialCharacters.Contains(
                    character))
            {
                result.Append('\\');
            }

            result.Append(character);
        }

        return result.ToString();
    }

    // ============================================================
    // Diagnostic
    // ============================================================

    private static MetadataSearchProviderResult
        CreateDiagnosticResult(
            MetadataSearchRequest request,
            string message)
    {
        return new MetadataSearchProviderResult
        {
            Source =
                "MusicBrainz Diagnostic",

            Artist =
                request.Artist,

            Title =
                request.Title,

            Confidence =
                0,

            MatchReason =
                message
        };
    }

    // ============================================================
    // HTTP
    // ============================================================

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
    // Rate Limit
    // ============================================================

    private static async Task WaitForRateLimitAsync(
        CancellationToken cancellationToken)
    {
        await RateLimit.WaitAsync(
            cancellationToken);

        try
        {
            var elapsed =
                DateTime.UtcNow -
                _lastRequestUtc;

            var minimumInterval =
                TimeSpan.FromSeconds(1);

            if (elapsed < minimumInterval)
            {
                await Task.Delay(
                    minimumInterval - elapsed,
                    cancellationToken);
            }

            _lastRequestUtc =
                DateTime.UtcNow;
        }
        finally
        {
            _lastRequestUtc =
                DateTime.UtcNow;

            RateLimit.Release();
        }
    }
}