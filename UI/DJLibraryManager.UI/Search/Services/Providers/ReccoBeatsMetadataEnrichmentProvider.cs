using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace DJLibraryManager.UI.Search.Services.Providers;

/// <summary>
/// Performs second-stage metadata enrichment using ReccoBeats.
///
/// Unlike the primary ReccoBeats metadata provider, this provider
/// does not perform an Artist + Title search.
///
/// The primary search has already established the recording and
/// supplied the ReccoBeats provider identity. This provider uses
/// that established ReccoBeats ExternalId to retrieve additional
/// metadata for the same recording.
///
/// ReccoBeats is therefore an enrichment source rather than another
/// candidate-discovery source.
///
/// The provider does not modify the DIASISS library.
/// </summary>
public sealed class ReccoBeatsMetadataEnrichmentProvider
    : IMetadataEnrichmentProvider
{
    private const string BaseUrl =
        "https://api.reccobeats.com/v1";

    private readonly HttpClient _httpClient;

    public ReccoBeatsMetadataEnrichmentProvider()
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

    // ============================================================
    // Provider Identity
    // ============================================================

    public string Name =>
        "ReccoBeats";

    // ============================================================
    // Enrichment
    // ============================================================

    /// <summary>
    /// Enriches an already-established ReccoBeats recording.
    ///
    /// No Artist + Title search is performed here.
    ///
    /// The ReccoBeats ExternalId established during the primary
    /// search is used to retrieve additional metadata.
    /// </summary>
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

        //
        // Find the ReccoBeats identity established by the
        // primary search.
        //

        var providerIdentity =
            request.ProviderIdentities?
                .FirstOrDefault(
                    identity =>
                        identity is not null &&
                        string.Equals(
                            identity.Provider,
                            Name,
                            StringComparison.OrdinalIgnoreCase));

        if (providerIdentity is null ||
            string.IsNullOrWhiteSpace(
                providerIdentity.ExternalId))
        {
            //
            // This is important.
            //
            // We deliberately do NOT fall back to Artist + Title
            // searching. If the primary search did not establish
            // a ReccoBeats identity, this enrichment provider has
            // no safely identified recording to enrich.
            //

            return [];
        }

        var trackId =
            providerIdentity.ExternalId.Trim();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            //
            // Determine which API resources are actually required.
            //

            var needsTrackDetails =
                NeedsAnyField(
                    request,
                    "Album",
                    "Year",
                    "ReleaseYear",
                    "Duration");

            var needsAudioFeatures =
                NeedsAnyField(
                    request,
                    "BPM",
                    "Key");

            JsonElement? trackDetails = null;
            JsonElement? albumDetails = null;
            JsonElement? audioFeatures = null;

            //
            // Track details.
            //

            if (needsTrackDetails)
            {
                trackDetails =
                    await GetTrackDetailsAsync(
                        trackId,
                        cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            //
            // Album information.
            //
            // The track's album is retrieved independently from the
            // track details because ReccoBeats exposes a dedicated
            // track -> album endpoint.
            //

            if (NeedsAnyField(
                    request,
                    "Album",
                    "Year",
                    "ReleaseYear"))
            {
                albumDetails =
                    await GetTrackAlbumAsync(
                        trackId,
                        cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            //
            // Audio features.
            //

            if (needsAudioFeatures)
            {
                audioFeatures =
                    await GetAudioFeaturesAsync(
                        trackId,
                        cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            //
            // Extract only the fields that were actually requested.
            //

            var album =
                HasField(
                    request,
                    "Album")
                    ? GetAlbumTitle(
                        albumDetails,
                        trackDetails)
                    : string.Empty;

            var releaseYear =
                HasField(
                    request,
                    "ReleaseYear")
                    ? GetReleaseYear(
                        albumDetails)
                    : null;

            //
            // We intentionally do not populate Year from the album
            // release date.
            //
            // MetadataSearchProviderResult distinguishes track Year
            // from ReleaseYear. An album/release date is not proof of
            // the track's original year.
            //

            var year =
                HasField(
                    request,
                    "Year")
                    ? GetTrackYear(
                        trackDetails)
                    : null;

            var bpm =
                HasField(
                    request,
                    "BPM")
                    ? GetNullableDouble(
                        audioFeatures,
                        "tempo")
                    : null;

            var key =
                HasField(
                    request,
                    "Key")
                    ? BuildMusicalKey(
                        audioFeatures)
                    : string.Empty;

            var duration =
                HasField(
                    request,
                    "Duration")
                    ? GetDuration(
                        trackDetails)
                    : null;

            //
            // ReccoBeats does not currently provide reliable
            // track-level genre information through this API.
            //
            // Leave Genre empty so MusicBrainz / Discogs can supply
            // genre evidence independently.
            //

            var hasEvidence =
                !string.IsNullOrWhiteSpace(album) ||
                year.HasValue ||
                releaseYear.HasValue ||
                bpm.HasValue ||
                !string.IsNullOrWhiteSpace(key) ||
                duration.HasValue;

            if (!hasEvidence)
            {
                return [];
            }

            return
            [
                new MetadataSearchProviderResult
                {
                    Source =
                        Name,

                    //
                    // This is the established recording identity.
                    //
                    ExternalId =
                        trackId,

                    //
                    // Artist and Title are intentionally NOT returned
                    // as enrichment evidence.
                    //
                    // They were already established by the primary
                    // search and should not be reintroduced into the
                    // Artist/Title consensus calculation.
                    //
                    Artist =
                        string.Empty,

                    Title =
                        string.Empty,

                    Album =
                        album,

                    Genre =
                        string.Empty,

                    Year =
                        year,

                    ReleaseYear =
                        releaseYear,

                    BPM =
                        bpm,

                    Key =
                        key,

                    Duration =
                        duration,

                    //
                    // The recording ID was established during the
                    // primary search, so this is not a new candidate
                    // match. The returned evidence belongs to that
                    // already-established recording.
                    //
                    Confidence =
                        100.0,

                    MatchReason =
                        BuildMatchReason(
                            request,
                            album,
                            year,
                            releaseYear,
                            bpm,
                            key,
                            duration)
                }
            ];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            //
            // Enrichment providers are independent.
            //
            // A ReccoBeats failure must not prevent MusicBrainz,
            // Discogs or another enrichment provider from returning
            // evidence.
            //

            return [];
        }
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
            $"{BaseUrl}/track/" +
            $"{Uri.EscapeDataString(trackId)}";

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
    // Track Album
    // ============================================================

    private async Task<JsonElement?>
        GetTrackAlbumAsync(
            string trackId,
            CancellationToken cancellationToken)
    {
        var url =
            $"{BaseUrl}/track/" +
            $"{Uri.EscapeDataString(trackId)}" +
            "/album";

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
    // Album
    // ============================================================

    private static string GetAlbumTitle(
        JsonElement? album,
        JsonElement? track)
    {
        var albumTitle =
            GetString(
                album,
                "albumTitle");

        if (!string.IsNullOrWhiteSpace(
                albumTitle))
        {
            return albumTitle;
        }

        //
        // Some responses may expose album information directly
        // inside the track object.
        //

        var nestedAlbum =
            GetProperty(
                track,
                "album");

        if (nestedAlbum.HasValue)
        {
            albumTitle =
                GetString(
                    nestedAlbum,
                    "albumTitle");

            if (!string.IsNullOrWhiteSpace(
                    albumTitle))
            {
                return albumTitle;
            }

            albumTitle =
                GetString(
                    nestedAlbum,
                    "name");

            if (!string.IsNullOrWhiteSpace(
                    albumTitle))
            {
                return albumTitle;
            }
        }

        //
        // Some API responses may return "name" at the album level.
        //

        return
            GetString(
                album,
                "name")
            ?? string.Empty;
    }

    // ============================================================
    // Year
    // ============================================================

    private static int? GetTrackYear(
        JsonElement? track)
    {
        //
        // Only use an explicit track-level year.
        //
        // Do NOT treat album releaseDate as the track Year.
        //

        var year =
            GetNullableInt(
                track,
                "year");

        if (year.HasValue)
        {
            return year;
        }

        var releaseYear =
            GetNullableInt(
                track,
                "releaseYear");

        if (releaseYear.HasValue)
        {
            return releaseYear;
        }

        return null;
    }

    private static int? GetReleaseYear(
        JsonElement? album)
    {
        var releaseDate =
            GetString(
                album,
                "releaseDate");

        if (string.IsNullOrWhiteSpace(
                releaseDate))
        {
            return null;
        }

        //
        // ReccoBeats releaseDate is expected to be an ISO-style
        // date, but only the year is relevant to our model.
        //

        if (releaseDate.Length >= 4 &&
            int.TryParse(
                releaseDate[..4],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var year))
        {
            if (year >= 1000 &&
                year <= 9999)
            {
                return year;
            }
        }

        if (DateTime.TryParse(
                releaseDate,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            return parsedDate.Year;
        }

        return null;
    }

    // ============================================================
    // Duration
    // ============================================================

    private static TimeSpan? GetDuration(
        JsonElement? track)
    {
        var durationMs =
            GetNullableDouble(
                track,
                "durationMs");

        if (!durationMs.HasValue ||
            durationMs.Value <= 0)
        {
            return null;
        }

        return
            TimeSpan.FromMilliseconds(
                durationMs.Value);
    }

    // ============================================================
    // Musical Key
    // ============================================================

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

        return
            mode.Value == 1
                ? majorKeys[key.Value]
                : minorKeys[key.Value];
    }

    // ============================================================
    // Field Helpers
    // ============================================================

    private static bool HasField(
        MetadataEnrichmentRequest request,
        string field)
    {
        return
            request.MissingFields
                .Any(
                    missingField =>
                        string.Equals(
                            missingField,
                            field,
                            StringComparison.OrdinalIgnoreCase));
    }

    private static bool NeedsAnyField(
        MetadataEnrichmentRequest request,
        params string[] fields)
    {
        foreach (var field in fields)
        {
            if (HasField(
                    request,
                    field))
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // Match Reason
    // ============================================================

    private static string BuildMatchReason(
        MetadataEnrichmentRequest request,
        string album,
        int? year,
        int? releaseYear,
        double? bpm,
        string key,
        TimeSpan? duration)
    {
        var reasons =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                album))
        {
            reasons.Add(
                "Album supplied by ReccoBeats");
        }

        if (year.HasValue)
        {
            reasons.Add(
                $"Track year {year.Value}");
        }

        if (releaseYear.HasValue)
        {
            reasons.Add(
                $"Release year {releaseYear.Value}");
        }

        if (bpm.HasValue)
        {
            reasons.Add(
                $"ReccoBeats BPM {bpm.Value:0.###}");
        }

        if (!string.IsNullOrWhiteSpace(
                key))
        {
            reasons.Add(
                $"ReccoBeats key {key}");
        }

        if (duration.HasValue)
        {
            reasons.Add(
                $"Duration {duration.Value:mm\\:ss}");
        }

        if (reasons.Count == 0)
        {
            return
                "ReccoBeats enrichment evidence.";
        }

        return
            "Established ReccoBeats recording: " +
            string.Join(
                "; ",
                reasons);
    }

    // ============================================================
    // JSON Helpers
    // ============================================================

    private static JsonElement? GetProperty(
        JsonElement? element,
        string propertyName)
    {
        if (!element.HasValue)
        {
            return null;
        }

        if (!element.Value.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        return property;
    }

    private static string? GetString(
        JsonElement? element,
        string propertyName)
    {
        if (!element.HasValue)
        {
            return null;
        }

        if (!element.Value.TryGetProperty(
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

        if (!element.Value.TryGetProperty(
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

        if (!element.Value.TryGetProperty(
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
}