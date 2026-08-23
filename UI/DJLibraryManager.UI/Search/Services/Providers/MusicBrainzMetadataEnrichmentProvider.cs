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
/// Performs second-stage metadata enrichment using MusicBrainz.
///
/// The primary search establishes the recording identity.
/// This provider uses that established identity to discover
/// additional metadata for fields that remain unresolved.
///
/// Enrichment is discovery only. It does not modify the
/// DIASISS library or physical media files.
/// </summary>
public sealed class MusicBrainzMetadataEnrichmentProvider
    : IMetadataEnrichmentProvider
{
    private const string SearchApiBaseUrl =
        "https://musicbrainz.org/ws/2/recording";

    private const string UserAgent =
        "DIASISS/0.1.0 (DJ Library Manager)";

    private const int SearchResultLimit = 25;

    private const int CandidateLookupLimit = 5;

    private const double MinimumCandidateScore = 70.0;

    private static readonly HttpClient HttpClient =
        CreateHttpClient();

    private static readonly SemaphoreSlim RateLimit =
        new(1, 1);

    private static DateTime _lastRequestUtc =
        DateTime.MinValue;

    public string Name =>
        "MusicBrainz";

    // ============================================================
    // Enrichment
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

        if (string.IsNullOrWhiteSpace(request.Artist) &&
            string.IsNullOrWhiteSpace(request.Title))
        {
            return [];
        }

        /*
         * IMPORTANT:
         *
         * Artist and Title here are NOT the original local
         * library values.
         *
         * They are the identity already established by the
         * primary metadata search.
         *
         * MusicBrainz is therefore being used as an enrichment
         * source rather than another identity resolver.
         */

        var recordings =
            await SearchRecordingsAsync(
                request,
                cancellationToken);

        if (recordings.Count == 0)
        {
            return [];
        }

        var candidates =
            recordings
                .Select(
                    recording =>
                        CreateCandidate(
                            recording,
                            request))
                .Where(
                    candidate =>
                        candidate is not null)
                .Select(
                    candidate =>
                        candidate!)
                .Where(
                    candidate =>
                        candidate.Score >=
                        MinimumCandidateScore)
                .OrderByDescending(
                    candidate =>
                        candidate.Score)
                .ThenByDescending(
                    candidate =>
                        candidate.DurationMatch)
                .Take(
                    CandidateLookupLimit)
                .ToList();

        if (candidates.Count == 0)
        {
            return [];
        }

        var results =
            new List<MetadataSearchProviderResult>();

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result =
                await LookupRecordingAsync(
                    candidate.RecordingId,
                    request,
                    cancellationToken);

            if (result is not null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    // ============================================================
    // Search Recordings
    // ============================================================

    private static async Task<
        List<JsonElement>>
        SearchRecordingsAsync(
            MetadataEnrichmentRequest request,
            CancellationToken cancellationToken)
    {
        var title =
            CleanTitleForSearch(
                request.Title);

        var artist =
            CleanArtistForSearch(
                request.Artist);

        if (string.IsNullOrWhiteSpace(title) &&
            string.IsNullOrWhiteSpace(artist))
        {
            return [];
        }

        var queries =
            BuildSearchQueries(
                artist,
                title);

        var recordings =
            new List<JsonElement>();

        var seenIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var query in queries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var url =
                $"{SearchApiBaseUrl}" +
                $"?query={Uri.EscapeDataString(query)}" +
                $"&limit={SearchResultLimit}" +
                "&inc=artist-credits" +
                "&fmt=json";

            await WaitForRateLimitAsync(
                cancellationToken);

            using var response =
                await HttpClient.GetAsync(
                    url,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            try
            {
                using var document =
                    JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty(
                        "recordings",
                        out var recordingArray) ||
                    recordingArray.ValueKind !=
                        JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var recording in
                         recordingArray.EnumerateArray())
                {
                    var id =
                        GetString(
                            recording,
                            "id");

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    if (!seenIds.Add(id))
                    {
                        continue;
                    }

                    recordings.Add(
                        recording.Clone());
                }
            }
            catch (JsonException)
            {
                continue;
            }

            /*
             * We deliberately allow several queries to contribute
             * candidates. The first MusicBrainz search results are
             * not necessarily the correct recording.
             */
        }

        return recordings;
    }

    // ============================================================
    // Search Query Construction
    // ============================================================

    private static List<string> BuildSearchQueries(
        string artist,
        string title)
    {
        var queries =
            new List<string>();

        /*
         * First search:
         *
         * Search by the established title.
         *
         * This gives us broad recall.
         */
        if (!string.IsNullOrWhiteSpace(title))
        {
            queries.Add(
                $"recording:\"{EscapeLucene(title)}\"");
        }

        /*
         * Second stage:
         *
         * Search using the individual established artist
         * components rather than treating a collaboration as
         * one literal artist name.
         *
         * Example:
         *
         *     Luude, Colin Hay
         *
         * becomes:
         *
         *     artist:"Luude" AND recording:"Down Under"
         *
         * and:
         *
         *     artist:"Colin Hay" AND recording:"Down Under"
         */
        var artistComponents =
            SplitArtistComponents(
                artist);

        foreach (var artistComponent in artistComponents)
        {
            if (string.IsNullOrWhiteSpace(
                    artistComponent) ||
                string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            queries.Add(
                $"artist:\"{EscapeLucene(artistComponent)}\" " +
                $"AND recording:\"{EscapeLucene(title)}\"");
        }

        /*
         * Final combined search.
         *
         * This is intentionally less restrictive than the old
         * artist:"Luude, Colin Hay" query.
         */
        if (!string.IsNullOrWhiteSpace(artist) &&
            !string.IsNullOrWhiteSpace(title))
        {
            var firstArtist =
                artistComponents.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(firstArtist))
            {
                queries.Add(
                    $"artist:\"{EscapeLucene(firstArtist)}\" " +
                    $"AND recording:\"{EscapeLucene(title)}\"");
            }
        }

        return queries
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ============================================================
    // Artist Components
    // ============================================================

    private static List<string> SplitArtistComponents(
        string? artist)
    {
        if (string.IsNullOrWhiteSpace(artist))
        {
            return [];
        }

        var cleaned =
            artist.Trim();

        var parts =
            cleaned
                .Split(
                    new[]
                    {
                        ',',
                        ';',
                        '&'
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    part =>
                        part.Trim())
                .Where(
                    part =>
                        !string.IsNullOrWhiteSpace(part))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        /*
         * If there were no separators, use the whole artist.
         */
        if (parts.Count == 0)
        {
            parts.Add(cleaned);
        }

        return parts;
    }

    // ============================================================
    // Candidate
    // ============================================================

    private sealed record EnrichmentCandidate(
        string RecordingId,
        double Score,
        bool DurationMatch);

    private static EnrichmentCandidate?
        CreateCandidate(
            JsonElement recording,
            MetadataEnrichmentRequest request)
    {
        var recordingId =
            GetString(
                recording,
                "id");

        if (string.IsNullOrWhiteSpace(
                recordingId))
        {
            return null;
        }

        var artist =
            GetArtistCredit(
                recording);

        var title =
            GetString(
                recording,
                "title");

        if (string.IsNullOrWhiteSpace(
                artist) ||
            string.IsNullOrWhiteSpace(
                title))
        {
            return null;
        }

        var artistScore =
            CalculateArtistScore(
                request.Artist,
                artist);

        var titleScore =
            CalculateTitleScore(
                request.Title,
                title);

        var duration =
            GetDuration(
                recording);

        var durationMatch =
            false;

        var durationScore =
            0.0;

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
                durationScore = 15;
                durationMatch = true;
            }
            else if (difference <= 5)
            {
                durationScore = 10;
            }
            else if (difference <= 10)
            {
                durationScore = 5;
            }
        }

        /*
         * Artist and Title are the primary identity.
         *
         * Duration is supporting evidence only.
         */
        var score =
            artistScore +
            titleScore +
            durationScore;

        return new EnrichmentCandidate(
            recordingId,
            score,
            durationMatch);
    }

    // ============================================================
    // Recording Lookup
    // ============================================================

    private static async Task<
        MetadataSearchProviderResult?>
        LookupRecordingAsync(
            string recordingId,
            MetadataEnrichmentRequest request,
            CancellationToken cancellationToken)
    {
        var url =
            $"{SearchApiBaseUrl}/" +
            $"{Uri.EscapeDataString(recordingId)}" +
            "?inc=artist-credits+releases+genres" +
            "&fmt=json";

        await WaitForRateLimitAsync(
            cancellationToken);

        using var response =
            await HttpClient.GetAsync(
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

            var recording =
                document.RootElement;

            /*
             * We have already established the identity before
             * entering this provider.
             *
             * Therefore we deliberately do NOT use the MusicBrainz
             * artist/title as the returned identity.
             *
             * MusicBrainz artist/title are used only to validate
             * the candidate.
             */
            var returnedArtist =
                GetArtistCredit(
                    recording);

            var returnedTitle =
                GetString(
                    recording,
                    "title");

            if (!ArtistMatches(
                    request.Artist,
                    returnedArtist))
            {
                return null;
            }

            if (!TitleMatches(
                    request.Title,
                    returnedTitle))
            {
                return null;
            }

            var year =
                HasMissingField(
                    request,
                    "Year")
                    ? GetFirstReleaseYear(
                        recording)
                    : null;

            var releaseYear =
                HasMissingField(
                    request,
                    "ReleaseYear")
                    ? GetFirstReleaseYear(
                        recording)
                    : null;

            var genre =
                HasMissingField(
                    request,
                    "Genre")
                    ? GetGenre(
                        recording)
                    : string.Empty;

            var duration =
                GetDuration(
                    recording);

            /*
             * If MusicBrainz cannot provide any of the fields
             * we actually asked it to enrich, there is no useful
             * evidence to return.
             */
            if (!year.HasValue &&
                !releaseYear.HasValue &&
                string.IsNullOrWhiteSpace(
                    genre))
            {
                return null;
            }

            return new MetadataSearchProviderResult
            {
                Source =
                    "MusicBrainz",

                ExternalId =
                    recordingId,

                /*
                 * IMPORTANT:
                 *
                 * Preserve the established identity.
                 */
                Artist =
                    request.Artist,

                Title =
                    request.Title,

                /*
                 * Album is only supplementary provider evidence.
                 */
                Album =
                    HasMissingField(
                        request,
                        "Album")
                        ? GetFirstReleaseTitle(
                            recording)
                        : request.Album,

                Genre =
                    genre,

                Year =
                    year,

                ReleaseYear =
                    releaseYear,

                Duration =
                    duration,

                Confidence =
                    CalculateConfidence(
                        request,
                        returnedArtist,
                        returnedTitle,
                        duration),

                MatchReason =
                    BuildMatchReason(
                        request,
                        returnedArtist,
                        returnedTitle,
                        year,
                        releaseYear,
                        genre,
                        duration)
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ============================================================
    // Missing Field
    // ============================================================

    private static bool HasMissingField(
        MetadataEnrichmentRequest request,
        string field)
    {
        return request.MissingFields
            .Any(
                missingField =>
                    string.Equals(
                        missingField,
                        field,
                        StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Genre
    // ============================================================

    private static string GetGenre(
        JsonElement recording)
    {
        if (!recording.TryGetProperty(
                "genres",
                out var genres) ||
            genres.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        var candidates =
            new List<(string Name, int Count)>();

        foreach (var genre in
                 genres.EnumerateArray())
        {
            var name =
                GetString(
                    genre,
                    "name");

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var count =
                GetInt32(
                    genre,
                    "count");

            candidates.Add(
                (
                    name.Trim(),
                    count));
        }

        return candidates
            .OrderByDescending(
                candidate =>
                    candidate.Count)
            .ThenBy(
                candidate =>
                    candidate.Name,
                StringComparer.OrdinalIgnoreCase)
            .Select(
                candidate =>
                    candidate.Name)
            .FirstOrDefault()
            ?? string.Empty;
    }

    // ============================================================
    // Artist
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
                artists.Add(
                    name);
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
                "releases",
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
                out var length) ||
            length.ValueKind !=
                JsonValueKind.Number)
        {
            return null;
        }

        if (!length.TryGetInt64(
                out var milliseconds) ||
            milliseconds <= 0)
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
        MetadataEnrichmentRequest request,
        string artist,
        string title,
        TimeSpan? duration)
    {
        var confidence =
            50.0;

        confidence +=
            CalculateArtistScore(
                request.Artist,
                artist) * 0.30;

        confidence +=
            CalculateTitleScore(
                request.Title,
                title) * 0.30;

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
                confidence += 20;
            }
            else if (difference <= 5)
            {
                confidence += 12;
            }
            else if (difference <= 10)
            {
                confidence += 5;
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
    // Artist Score
    // ============================================================

    private static double CalculateArtistScore(
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
            return 0;
        }

        var matched =
            requestedTokens.Count(
                token =>
                    returnedTokens.Contains(
                        token,
                        StringComparer.OrdinalIgnoreCase));

        var ratio =
            (double)matched /
            requestedTokens.Count;

        /*
         * 70 points represents a complete artist match.
         */
        return ratio * 70.0;
    }

    // ============================================================
    // Title Score
    // ============================================================

    private static double CalculateTitleScore(
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
            return 0;
        }

        var requestedNormalised =
            Normalise(
                requestedTitle);

        var returnedNormalised =
            Normalise(
                returnedTitle);

        if (string.Equals(
                requestedNormalised,
                returnedNormalised,
                StringComparison.OrdinalIgnoreCase))
        {
            return 100.0;
        }

        if (requestedNormalised.Contains(
                returnedNormalised,
                StringComparison.OrdinalIgnoreCase) ||
            returnedNormalised.Contains(
                requestedNormalised,
                StringComparison.OrdinalIgnoreCase))
        {
            return 80.0;
        }

        return 0;
    }

    // ============================================================
    // Artist Matching
    // ============================================================

    private static bool ArtistMatches(
        string? requested,
        string? returned)
    {
        return CalculateArtistScore(
                   requested,
                   returned) >= 50.0;
    }

    // ============================================================
    // Title Matching
    // ============================================================

    private static bool TitleMatches(
        string? requested,
        string? returned)
    {
        return CalculateTitleScore(
                   requested,
                   returned) >= 80.0;
    }

    // ============================================================
    // Match Reason
    // ============================================================

    private static string BuildMatchReason(
        MetadataEnrichmentRequest request,
        string artist,
        string title,
        int? year,
        int? releaseYear,
        string genre,
        TimeSpan? duration)
    {
        var parts =
            new List<string>
            {
                "MusicBrainz enrichment",
                "Established artist identity matched",
                "Established title identity matched"
            };

        if (year.HasValue)
        {
            parts.Add(
                $"Year found: {year.Value}");
        }

        if (releaseYear.HasValue)
        {
            parts.Add(
                $"Release year found: {releaseYear.Value}");
        }

        if (!string.IsNullOrWhiteSpace(
                genre))
        {
            parts.Add(
                $"Genre found: {genre}");
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

        var lastDot =
            title.LastIndexOf('.');

        if (lastDot > 0 &&
            lastDot >= title.Length - 5)
        {
            title =
                title[..lastDot];
        }

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

        title =
            System.Text.RegularExpressions.Regex.Replace(
                title,
                @"\s+(?:\d{1,2}(?:\.\d+)?[""″]|\d{1,2}\s*(?:inch|in))\s*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        title =
            System.Text.RegularExpressions.Regex.Replace(
                title,
                @"\s*\([^()]*\)\s*$",
                string.Empty);

        return title
            .Trim()
            .Trim(
                '-',
                '_',
                '–',
                '—')
            .Trim();
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

    private static int GetInt32(
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
            property.TryGetInt32(
                out var value))
        {
            return value;
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

            result.Append(
                character);
        }

        return result.ToString();
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