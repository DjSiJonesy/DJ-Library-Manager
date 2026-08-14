using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Searches FreqBlog for DJ-oriented audio metadata.
///
/// FreqBlog is used as a discovery and audio-analysis source only.
/// It never modifies the DIASISS library or physical media files.
///
/// FreqBlog is particularly useful for DJ metadata such as:
///
/// - BPM
/// - Alternative / half-time / double-time BPM
/// - Musical Key
/// - Camelot Key
/// - Duration
/// - ISRC
/// - MusicBrainz ID
/// - Remix information
///
/// Authentication is supplied through the
/// FREQBLOG_API_KEY environment variable.
/// </summary>
public sealed class FreqBlogMetadataProvider
    : IMetadataSearchProvider
{
    private const string ApiBaseUrl =
        "https://api.freqblog.com";

    private const int MaximumResults = 5;

    private static readonly HttpClient HttpClient =
        CreateHttpClient();

    public string Name =>
        "FreqBlog";

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

        var apiKey =
            Environment.GetEnvironmentVariable(
                "FREQBLOG_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "FREQBLOG_API_KEY environment variable " +
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
                    "No Artist or Title was supplied to FreqBlog.")
            ];
        }

        var url =
            BuildLookupUrl(request);

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        httpRequest.Headers.TryAddWithoutValidation(
            "X-Api-Key",
            apiKey);

        using var response =
            await HttpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "FreqBlog did not find this track.")
            ];
        }

        if (!response.IsSuccessStatusCode)
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    $"FreqBlog returned HTTP " +
                    $"{(int)response.StatusCode}.")
            ];
        }

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var document =
            await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

        return ParseResult(
            document.RootElement,
            request);
    }

    // ============================================================
    // URL
    // ============================================================

    private static string BuildLookupUrl(
        MetadataSearchRequest request)
    {
        var parameters =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                request.Title))
        {
            parameters.Add(
                "track=" +
                Uri.EscapeDataString(
                    request.Title.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(
                request.Artist))
        {
            parameters.Add(
                "artist=" +
                Uri.EscapeDataString(
                    request.Artist.Trim()));
        }

        return
            $"{ApiBaseUrl}/lookup?" +
            string.Join(
                "&",
                parameters);
    }

    // ============================================================
    // Parse
    // ============================================================

    private static IReadOnlyList<
        MetadataSearchProviderResult>
        ParseResult(
            JsonElement root,
            MetadataSearchRequest request)
    {
        if (root.ValueKind !=
            JsonValueKind.Object)
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "FreqBlog returned an invalid JSON response.")
            ];
        }

        var trackName =
            GetString(
                root,
                "track_name");

        var artistName =
            GetString(
                root,
                "artist_name");

        if (string.IsNullOrWhiteSpace(trackName) &&
            string.IsNullOrWhiteSpace(artistName))
        {
            return
            [
                CreateDiagnosticResult(
                    request,
                    "FreqBlog returned no usable track identity.")
            ];
        }

        var album =
            GetString(
                root,
                "album_name");

        var key =
            GetString(
                root,
                "key");

        var camelot =
            GetString(
                root,
                "camelot");

        var bpm =
            GetDouble(
                root,
                "bpm");

        var duration =
            GetDuration(
                root,
                "duration_ms");

        var releaseDate =
            GetString(
                root,
                "release_date");

        var year =
            ParseYear(
                releaseDate);

        var isrc =
            GetString(
                root,
                "isrc");

        var mbid =
            GetString(
                root,
                "mbid");

        var externalId =
            !string.IsNullOrWhiteSpace(isrc)
                ? isrc
                : mbid;

        var bpmConfidence =
            GetDouble(
                root,
                "bpm_confidence");

        var keyConfidence =
            GetDouble(
                root,
                "key_confidence");

        var confidence =
            CalculateConfidence(
                request,
                artistName,
                trackName,
                bpmConfidence,
                keyConfidence);

        var reason =
            BuildMatchReason(
                request,
                artistName,
                trackName,
                bpm,
                key,
                camelot,
                duration,
                bpmConfidence,
                keyConfidence);

        return
        [
            new MetadataSearchProviderResult
            {
                Source =
                    "FreqBlog",

                ExternalId =
                    externalId,

                Artist =
                    artistName,

                Title =
                    trackName,

                Album =
                    album,

                Genre =
                    GetString(
                        root,
                        "genre"),

                Year =
                    year,

                ReleaseYear =
                    year,

                BPM =
                    bpm,

                Key =
                    BuildKeyDisplay(
                        key,
                        camelot),

                Duration =
                    duration,

                Confidence =
                    confidence,

                MatchReason =
                    reason
            }
        ];
    }

    // ============================================================
    // Confidence
    // ============================================================

    private static double CalculateConfidence(
        MetadataSearchRequest request,
        string artist,
        string title,
        double? bpmConfidence,
        double? keyConfidence)
    {
        var score = 0.0;

        if (ExactMatch(
                request.Artist,
                artist))
        {
            score += 45;
        }
        else if (ContainsMatch(
                     request.Artist,
                     artist))
        {
            score += 30;
        }

        if (ExactMatch(
                request.Title,
                title))
        {
            score += 45;
        }
        else if (ContainsMatch(
                     request.Title,
                     title))
        {
            score += 30;
        }

        if (bpmConfidence.HasValue)
        {
            score +=
                Math.Clamp(
                    bpmConfidence.Value,
                    0,
                    10);
        }

        if (keyConfidence.HasValue)
        {
            score +=
                Math.Clamp(
                    keyConfidence.Value * 5,
                    0,
                    5);
        }

        return Math.Clamp(
            score,
            0,
            100);
    }

    // ============================================================
    // Match Reason
    // ============================================================

    private static string BuildMatchReason(
        MetadataSearchRequest request,
        string artist,
        string title,
        double? bpm,
        string key,
        string camelot,
        TimeSpan? duration,
        double? bpmConfidence,
        double? keyConfidence)
    {
        var reasons =
            new List<string>();

        if (ExactMatch(
                request.Artist,
                artist))
        {
            reasons.Add(
                "Artist match");
        }

        if (ExactMatch(
                request.Title,
                title))
        {
            reasons.Add(
                "Title match");
        }

        if (bpm.HasValue)
        {
            reasons.Add(
                $"BPM {bpm.Value:0.##}");
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            reasons.Add(
                $"Key {key}");
        }

        if (!string.IsNullOrWhiteSpace(camelot))
        {
            reasons.Add(
                $"Camelot {camelot}");
        }

        if (duration.HasValue)
        {
            reasons.Add(
                $"Duration {duration.Value:mm\\:ss}");
        }

        if (bpmConfidence.HasValue)
        {
            reasons.Add(
                $"BPM confidence " +
                $"{bpmConfidence.Value:0.0}/10");
        }

        if (keyConfidence.HasValue)
        {
            reasons.Add(
                $"Key confidence " +
                $"{keyConfidence.Value:P0}");
        }

        if (reasons.Count == 0)
        {
            return
                "FreqBlog returned a possible match.";
        }

        return string.Join(
            " • ",
            reasons);
    }

    // ============================================================
    // Key
    // ============================================================

    private static string BuildKeyDisplay(
        string key,
        string camelot)
    {
        if (!string.IsNullOrWhiteSpace(key) &&
            !string.IsNullOrWhiteSpace(camelot))
        {
            return
                $"{key} ({camelot})";
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        return camelot;
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static bool ExactMatch(
        string? requested,
        string? returned)
    {
        if (string.IsNullOrWhiteSpace(requested) ||
            string.IsNullOrWhiteSpace(returned))
        {
            return false;
        }

        return string.Equals(
            Normalise(requested),
            Normalise(returned),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsMatch(
        string? requested,
        string? returned)
    {
        if (string.IsNullOrWhiteSpace(requested) ||
            string.IsNullOrWhiteSpace(returned))
        {
            return false;
        }

        var requestedValue =
            Normalise(requested);

        var returnedValue =
            Normalise(returned);

        return
            returnedValue.Contains(
                requestedValue,
                StringComparison.OrdinalIgnoreCase)
            ||
            requestedValue.Contains(
                returnedValue,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalise(
        string value)
    {
        return value
            .Trim()
            .ToUpperInvariant();
    }

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

        if (property.ValueKind !=
            JsonValueKind.String)
        {
            return string.Empty;
        }

        return property
            .GetString()
            ?.Trim()
            ?? string.Empty;
    }

    private static double? GetDouble(
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
                out var value))
        {
            return value;
        }

        return null;
    }

    private static TimeSpan? GetDuration(
        JsonElement element,
        string propertyName)
    {
        var milliseconds =
            GetDouble(
                element,
                propertyName);

        if (!milliseconds.HasValue ||
            milliseconds.Value <= 0)
        {
            return null;
        }

        return TimeSpan.FromMilliseconds(
            milliseconds.Value);
    }

    private static int? ParseYear(
        string releaseDate)
    {
        if (string.IsNullOrWhiteSpace(
                releaseDate) ||
            releaseDate.Length < 4)
        {
            return null;
        }

        if (int.TryParse(
                releaseDate[..4],
                out var year))
        {
            return year;
        }

        return null;
    }

    // ============================================================
    // Diagnostic Result
    // ============================================================

    private static MetadataSearchProviderResult
        CreateDiagnosticResult(
            MetadataSearchRequest request,
            string message)
    {
        return new MetadataSearchProviderResult
        {
            Source =
                "FreqBlog",

            Artist =
                request.Artist ?? string.Empty,

            Title =
                request.Title ?? string.Empty,

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
            new HttpClient
            {
                BaseAddress =
                    new Uri(ApiBaseUrl),

                Timeout =
                    TimeSpan.FromSeconds(30)
            };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "DIASISS/0.1.0 (DJ Library Manager)");

        return client;
    }
}