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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Searches Discogs for possible metadata matches.
///
/// Discogs search results represent releases. This provider therefore
/// does not treat a release search result as a track match.
///
/// Each candidate release is retrieved and its actual tracklist is
/// inspected. A result is only returned when the requested track can
/// be matched against a track within that release.
///
/// Discogs is a discovery source only. It never modifies the DIASISS
/// library or physical media files.
/// </summary>
public sealed class DiscogsMetadataProvider
    : IMetadataSearchProvider
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

        var token =
            Environment.GetEnvironmentVariable(
                "DISCOGS_API_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "DISCOGS_API_TOKEN environment variable " +
                    "was not found.")
            ];
        }

        if (string.IsNullOrWhiteSpace(request.Artist) &&
            string.IsNullOrWhiteSpace(request.Title))
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "No Artist or Title was supplied to Discogs.")
            ];
        }

        var searchResults =
            await SearchReleasesAsync(
                request,
                token,
                cancellationToken);

        if (searchResults.Count == 0)
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "Discogs returned no release candidates.")
            ];
        }

        var candidates =
            new List<MetadataSearchProviderResult>();

        var inspected =
            0;

        foreach (var searchResult in searchResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (inspected >= MaximumCandidatesToInspect)
                break;

            var releaseId =
                GetString(
                    searchResult,
                    "id");

            if (string.IsNullOrWhiteSpace(releaseId))
                continue;

            inspected++;

            var release =
                await GetReleaseAsync(
                    releaseId,
                    token,
                    cancellationToken);

                        if (!release.HasValue)
                            continue;

                        var releaseValue =
                            release.Value;

                        var matches =
                            FindMatchingTracks(
                                releaseValue,
                                request);

                        foreach (var match in matches)
                        {
                            var result =
                                CreateResult(
                                    releaseValue,
                                    match,
                                    request);

                            if (result is not null)
                            {
                                candidates.Add(result);
                            }
                        }
        }

        if (candidates.Count == 0)
        {
            return
            [
                CreateDiagnosticResult(
            request,
            $"Discogs found {searchResults.Count} release " +
            $"candidate(s), inspected {inspected}, but no " +
            $"track within those releases matched the " +
            $"requested Artist/Title. " +
            $"Requested Artist: '{request.Artist}'. " +
            $"Requested Title: '{request.Title}'.")
            ];
        }

        return
            candidates
                .OrderByDescending(
                    result => result.Confidence)
                .ThenBy(
                    result => result.Title,
                    StringComparer.OrdinalIgnoreCase)
                .Take(SearchResultLimit)
                .ToList();
    }

    // ============================================================
    // Discogs Release Search
    // ============================================================

    private static async Task<
        List<JsonElement>>
        SearchReleasesAsync(
            MetadataSearchRequest request,
            string token,
            CancellationToken cancellationToken)
    {
        var url =
            BuildSearchUrl(request);

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
                    result => result.Clone())
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    // ============================================================
    // Build Search URL
    // ============================================================

    private static string BuildSearchUrl(
        MetadataSearchRequest request)
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

        // --------------------------------------------------------
        // When both Artist and Title are known, use Discogs'
        // structured search parameters rather than a broad
        // free-text query.
        // --------------------------------------------------------

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

        // --------------------------------------------------------
        // If only one value exists, also provide q so Discogs has
        // something useful to search against.
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(artist) ||
            string.IsNullOrWhiteSpace(title))
        {
            var query =
                !string.IsNullOrWhiteSpace(title)
                    ? title
                    : artist;

            parameters.Add(
                $"q={Uri.EscapeDataString(query)}");
        }

        return
            $"{SearchUrl}?{string.Join(
                "&",
                parameters)}";
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
            return null;

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
    // Find Matching Tracks
    // ============================================================

    private static List<JsonElement>
        FindMatchingTracks(
            JsonElement release,
            MetadataSearchRequest request)
    {
        if (!release.TryGetProperty(
                "tracklist",
                out var tracklist) ||
            tracklist.ValueKind !=
                JsonValueKind.Array)
        {
            return [];
        }

        var matches =
            new List<(JsonElement Track, double Score)>();

        foreach (var track in
                 tracklist.EnumerateArray())
        {
            var type =
                GetString(
                    track,
                    "type_");

            // Discogs can include headings such as "Side A"
            // in the tracklist. These are not actual tracks.
            if (string.Equals(
                    type,
                    "heading",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var trackTitle =
                GetString(
                    track,
                    "title");

            if (string.IsNullOrWhiteSpace(
                    trackTitle))
            {
                continue;
            }

            var trackArtist =
                GetTrackArtist(
                    track,
                    release);

            var score =
                CalculateTrackMatchScore(
                    request,
                    trackArtist,
                    trackTitle);

            // ----------------------------------------------------
            // Do not accept weak release/track associations.
            //
            // The whole purpose of this provider revision is to
            // prevent vague Discogs releases becoming candidates.
            // ----------------------------------------------------

            if (score >= 60)
            {
                matches.Add(
                    (
                        track.Clone(),
                        score));
            }
        }

        return matches
            .OrderByDescending(
                match => match.Score)
            .Take(3)
            .Select(
                match => match.Track)
            .ToList();
    }

    // ============================================================
    // Track Match Score
    // ============================================================

    private static double
        CalculateTrackMatchScore(
            MetadataSearchRequest request,
            string trackArtist,
            string trackTitle)
    {
        var score =
            0d;

        var artistKnown =
            !string.IsNullOrWhiteSpace(
                request.Artist);

        var titleKnown =
            !string.IsNullOrWhiteSpace(
                request.Title);

        if (artistKnown)
        {
            if (ArtistMatches(
                    request.Artist,
                    trackArtist))
            {
                score += 50;
            }
            else
            {
                // If Artist is known and explicitly does not
                // match the Discogs track artist, reject it.
                return 0;
            }
        }

        if (titleKnown)
        {
            var titleScore =
                CalculateTitleScore(
                    request.Title,
                    trackTitle);

            score += titleScore;
        }

        return score;
    }

    // ============================================================
    // Title Score
    // ============================================================

    private static double
    CalculateTitleScore(
        string requested,
        string returned)
    {
        if (string.IsNullOrWhiteSpace(requested) ||
            string.IsNullOrWhiteSpace(returned))
        {
            return 0;
        }

        // ------------------------------------------------------------
        // Exact normalised comparison
        // ------------------------------------------------------------

        var requestedNormalised =
            NormaliseTitle(requested);

        var returnedNormalised =
            NormaliseTitle(returned);

        if (string.Equals(
                requestedNormalised,
                returnedNormalised,
                StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        // ------------------------------------------------------------
        // Compare the base track titles.
        //
        // This deliberately removes:
        //
        // - remix/version information
        // - 12", 7", inch etc.
        //
        // Example:
        //
        // Never Too Much (89' Remix) 12"
        //
        // becomes:
        //
        // Never Too Much
        // ------------------------------------------------------------

        var requestedBase =
            NormaliseTitle(
                StripVersionInformation(
                    requested));

        var returnedBase =
            NormaliseTitle(
                StripVersionInformation(
                    returned));

        if (string.IsNullOrWhiteSpace(
                requestedBase) ||
            string.IsNullOrWhiteSpace(
                returnedBase))
        {
            return 0;
        }

        // ------------------------------------------------------------
        // Exact base-title match.
        //
        // This is a strong match, but not the maximum score because
        // the version information itself has not yet been confirmed.
        // ------------------------------------------------------------

        if (string.Equals(
                requestedBase,
                returnedBase,
                StringComparison.OrdinalIgnoreCase))
        {
            return 45;
        }

        // ------------------------------------------------------------
        // Conservative containment check.
        //
        // This is deliberately weaker because:
        //
        // "Never Too Much"
        //
        // could otherwise incorrectly match:
        //
        // "Never Too Much Again"
        // ------------------------------------------------------------

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
    // Create Result
    // ============================================================

    private static MetadataSearchProviderResult?
        CreateResult(
            JsonElement release,
            JsonElement track,
            MetadataSearchRequest request)
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

        // --------------------------------------------------------
        // If Artist is known, the actual track artist must match.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                request.Artist) &&
            !ArtistMatches(
                request.Artist,
                trackArtist))
        {
            return null;
        }

        // --------------------------------------------------------
        // The title must also have a meaningful match.
        // --------------------------------------------------------

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

        var year =
            GetInt(
                release,
                "year");

        var duration =
            ParseDuration(
                GetString(
                    track,
                    "duration"));

        var genre =
            GetFirstArrayValue(
                release,
                "genres");

        var style =
            GetFirstArrayValue(
                release,
                "styles");

        var label =
            GetFirstLabel(
                release);

        var country =
            GetString(
                release,
                "country");

        var format =
            GetFirstFormat(
                release);

        var resourceUrl =
            GetString(
                release,
                "uri");

        var confidence =
            CalculateProviderConfidence(
                request,
                trackArtist,
                trackTitle,
                year,
                duration,
                release,
                titleScore);

        var reason =
            BuildMatchReason(
                request,
                trackArtist,
                trackTitle,
                year,
                duration,
                format,
                genre,
                style,
                label,
                country,
                releaseId);

        return new MetadataSearchProviderResult
        {
            Source =
                "Discogs",

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

            Year =
                null,

            ReleaseYear =
                year,

            BPM =
                null,

            Key =
                string.Empty,

            Duration =
                duration,

            Confidence =
                confidence,

            MatchReason =
                reason
        };
    }

    // ============================================================
    // Provider Confidence
    // ============================================================

    private static double
        CalculateProviderConfidence(
            MetadataSearchRequest request,
            string artist,
            string title,
            int? year,
            TimeSpan? duration,
            JsonElement release,
            double titleScore)
    {
        var score =
            0d;

        if (!string.IsNullOrWhiteSpace(
                request.Artist))
        {
            if (ArtistMatches(
                    request.Artist,
                    artist))
            {
                score += 40;
            }
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title))
        {
            score +=
                titleScore * 0.5;
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

        if (year.HasValue)
        {
            score += 5;
        }

        if (release.TryGetProperty(
                "tracklist",
                out var tracklist) &&
            tracklist.ValueKind ==
                JsonValueKind.Array)
        {
            score += 5;
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
        int? year,
        TimeSpan? duration,
        string format,
        string genre,
        string style,
        string label,
        string country,
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
                "Artist matched");
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title) &&
            CalculateTitleScore(
                request.Title,
                title) >= 45)
        {
            parts.Add(
                "Title matched");
        }

        if (year.HasValue)
        {
            parts.Add(
                $"Release year: {year.Value}");
        }

        if (duration.HasValue)
        {
            parts.Add(
                $"Track duration: " +
                $"{duration.Value:mm\\:ss}");
        }

        if (!string.IsNullOrWhiteSpace(
                format))
        {
            parts.Add(
                $"Format: {format}");
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

        if (!string.IsNullOrWhiteSpace(
                label))
        {
            parts.Add(
                $"Label: {label}");
        }

        if (!string.IsNullOrWhiteSpace(
                country))
        {
            parts.Add(
                $"Country: {country}");
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
        // ============================================================
        // First try the artist explicitly attached to the track.
        // ============================================================

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

        // ============================================================
        // Discogs does not necessarily repeat the release artist on
        // every track.
        //
        // If no track-level artist exists, fall back to the main
        // artist(s) of the release.
        // ============================================================

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

        // Handle common cases such as:
        //
        // "Luther Vandross"
        // "Vandross, Luther"
        //
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
                   requestedToken =>
                       returnedTokens.Contains(
                           requestedToken,
                           StringComparer.OrdinalIgnoreCase));
    }

    // ============================================================
    // Title Normalisation
    // ============================================================

    private static string NormaliseTitle(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        var cleaned =
            value
                .Trim()
                .Replace(
                    "″",
                    "\"");

        cleaned =
            Regex.Replace(
                cleaned,
                @"\s+",
                " ");

        return
            Normalise(
                cleaned);
    }

    // ============================================================
    // Version Information
    // ============================================================

    private static string StripVersionInformation(
    string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var result =
            value.Trim();

        // ------------------------------------------------------------
        // Remove common physical format suffixes FIRST.
        //
        // Examples:
        //
        // 12"
        // 7"
        // 12 inch
        // 12-inch
        // ------------------------------------------------------------

        result =
            Regex.Replace(
                result,
                @"\s+\d{1,2}\s*(?:""|" +
                "″" +
                @"|inch(?:es)?|in)\s*$",
                string.Empty,
                RegexOptions.IgnoreCase);

        result =
            Regex.Replace(
                result,
                @"\s+\d{1,2}\s*-\s*inch\s*$",
                string.Empty,
                RegexOptions.IgnoreCase);

        // ------------------------------------------------------------
        // Remove trailing parenthesised version information.
        //
        // Examples:
        //
        // (89' Remix)
        // (Remix '89)
        // (Radio Edit)
        // (12" Mix)
        // ------------------------------------------------------------

        result =
            Regex.Replace(
                result,
                @"\s*\([^()]*\)\s*$",
                string.Empty);

        // ------------------------------------------------------------
        // Remove common dash-separated version suffixes.
        // ------------------------------------------------------------

        result =
            Regex.Replace(
                result,
                @"\s+-\s+(?:remix|mix|edit|version|radio edit|"
                + @"extended|extended mix|club mix|"
                + @"instrumental|acoustic).*$",
                string.Empty,
                RegexOptions.IgnoreCase);

        return result.Trim();
    }

    // ============================================================
    // General Normalisation
    // ============================================================

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
    // Duration
    // ============================================================

    private static TimeSpan? ParseDuration(
    string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned =
            value.Trim();

        // ============================================================
        // Discogs track durations are normally expressed as:
        //
        //     M:SS
        //
        // For example:
        //
        //     6:44
        //
        // This MUST be handled before TimeSpan.TryParse().
        //
        // TimeSpan.TryParse("6:44") interprets it as 6 hours and
        // 44 minutes, which would incorrectly become 44:00 when
        // displayed as mm:ss.
        // ============================================================

        var parts =
            cleaned.Split(
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

        // ============================================================
        // Handle H:MM:SS if a provider supplies a three-part value.
        // ============================================================

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

        // ============================================================
        // Only fall back to the general TimeSpan parser after the
        // explicit music-duration formats have been checked.
        // ============================================================

        if (TimeSpan.TryParse(
                cleaned,
                CultureInfo.InvariantCulture,
                out var duration))
        {
            return duration;
        }

        return null;
    }

    // ============================================================
    // Release Metadata
    // ============================================================

    private static string GetFirstLabel(
        JsonElement release)
    {
        if (!release.TryGetProperty(
                "labels",
                out var labels) ||
            labels.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var label in
                 labels.EnumerateArray())
        {
            var name =
                GetString(
                    label,
                    "name");

            if (!string.IsNullOrWhiteSpace(
                    name))
            {
                return name;
            }
        }

        return string.Empty;
    }

    private static string GetFirstFormat(
        JsonElement release)
    {
        if (!release.TryGetProperty(
                "formats",
                out var formats) ||
            formats.ValueKind !=
                JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var format in
                 formats.EnumerateArray())
        {
            var name =
                GetString(
                    format,
                    "name");

            var descriptions =
                GetStringArray(
                    format,
                    "descriptions");

            if (!string.IsNullOrWhiteSpace(
                    name) &&
                descriptions.Count > 0)
            {
                return
                    $"{name} ({string.Join(
                        ", ",
                        descriptions)})";
            }

            if (!string.IsNullOrWhiteSpace(
                    name))
            {
                return name;
            }
        }

        return string.Empty;
    }

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

    private static List<string>
        GetStringArray(
            JsonElement element,
            string propertyName)
    {
        var values =
            new List<string>();

        if (!element.TryGetProperty(
                propertyName,
                out var property) ||
            property.ValueKind !=
                JsonValueKind.Array)
        {
            return values;
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
                    values.Add(
                        value);
                }
            }
        }

        return values;
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
                "Discogs Diagnostic",

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