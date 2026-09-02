using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using ClosedXML.Excel;

using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Models;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Exports Search workflow decisions and recommendations.
///
/// Export is deliberately read-only:
///
///     SearchIssue
///          ↓
///     SearchExportService
///          ↓
///     CSV / XLSX / JSON
///
/// The service does not modify the DIASISS library,
/// SearchIssue objects or SQLite.
///
/// The exported data represents what Search has found and
/// what the user has selected for further processing.
///
/// MediaId is the authoritative DIASISS media identity and
/// is preserved throughout the export pipeline.
/// </summary>
public sealed class SearchExportService
{
    // ============================================================
    // Public Export Methods
    // ============================================================

    /// <summary>
    /// Exports Search issues and their selected metadata
    /// recommendations to CSV.
    /// </summary>
    public async Task ExportCsvAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var materializedIssues =
            issues
                .Where(issue => issue is not null)
                .ToList();

        await Task.Run(() =>
        {
            var builder =
                new StringBuilder();

            WriteCsvHeader(builder);

            foreach (var issue in materializedIssues)
            {
                WriteIssueCsvRows(
                    builder,
                    issue);
            }

            File.WriteAllText(
                filePath,
                builder.ToString(),
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: true));
        });
    }

    /// <summary>
    /// Exports Search issues and their selected metadata
    /// recommendations to XLSX.
    /// </summary>
    public async Task ExportXlsxAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var materializedIssues =
            issues
                .Where(issue => issue is not null)
                .ToList();

        await Task.Run(() =>
        {
            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Search Export");

            WriteXlsxHeader(
                worksheet);

            var row =
                2;

            foreach (var issue in materializedIssues)
            {
                row =
                    WriteIssueXlsxRows(
                        worksheet,
                        row,
                        issue);
            }

            FormatWorksheet(
                worksheet);

            workbook.SaveAs(
                filePath);
        });
    }

    /// <summary>
    /// Exports Search issues and their selected metadata
    /// recommendations to JSON.
    /// </summary>
    public async Task ExportJsonAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var materializedIssues =
            issues
                .Where(issue => issue is not null)
                .ToList();

        var export =
            materializedIssues
                .Select(
                    CreateExportIssue)
                .ToList();

        await using var stream =
            File.Create(filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            export,
            JsonOptions);
    }

    // ============================================================
    // Selected Metadata Export
    // ============================================================

    /// <summary>
    /// Exports ONLY selected metadata changes to CSV.
    ///
    /// IMPORTANT:
    ///
    /// This is a track-level export.
    ///
    /// There is exactly ONE row per SearchIssue / track.
    ///
    /// Multiple selected metadata changes are consolidated onto
    /// that single row.
    ///
    /// A metadata recommendation is included only when:
    ///
    ///     IsSelected == true
    ///     IsChange == true
    ///
    /// The authoritative DIASISS MediaId comes from SearchIssue.
    ///
    /// SearchResult objects and duplicate information are never
    /// included by this export.
    /// </summary>
    public async Task ExportSelectedMetadataCsvAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var rows =
            GetSelectedMetadataRows(
                issues);

        await Task.Run(() =>
        {
            var builder =
                new StringBuilder();

            WriteSelectedMetadataCsvHeader(
                builder);

            foreach (var row in rows)
            {
                WriteSelectedMetadataCsvRow(
                    builder,
                    row);
            }

            File.WriteAllText(
                filePath,
                builder.ToString(),
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: true));
        });
    }

    /// <summary>
    /// Exports ONLY selected metadata changes to XLSX.
    ///
    /// There is exactly ONE row per track.
    ///
    /// Multiple selected metadata changes are consolidated onto
    /// that single row.
    /// </summary>
    public async Task ExportSelectedMetadataXlsxAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var rows =
            GetSelectedMetadataRows(
                issues);

        await Task.Run(() =>
        {
            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Metadata Export");

            WriteSelectedMetadataXlsxHeader(
                worksheet);

            var rowNumber =
                2;

            foreach (var row in rows)
            {
                WriteSelectedMetadataXlsxRow(
                    worksheet,
                    rowNumber,
                    row);

                rowNumber++;
            }

            FormatWorksheet(
                worksheet);

            workbook.SaveAs(
                filePath);
        });
    }

    /// <summary>
    /// Exports ONLY selected metadata changes to JSON.
    ///
    /// There is exactly ONE JSON object per track.
    ///
    /// Multiple selected metadata changes are consolidated into
    /// that single object.
    /// </summary>
    public async Task ExportSelectedMetadataJsonAsync(
        IEnumerable<SearchIssue> issues,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var rows =
            GetSelectedMetadataRows(
                issues);

        await using var stream =
            File.Create(filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            rows,
            JsonOptions);
    }

    // ============================================================
    // Selected Metadata Rows
    // ============================================================

    /// <summary>
    /// Creates one consolidated export row per track.
    ///
    /// Every selected metadata recommendation is applied to the
    /// appropriate field on that single export row.
    ///
    /// For example, if Artist, Album, Genre and Year are selected,
    /// all four changes appear on the same row.
    ///
    /// The original value remains available in the Current fields
    /// and the selected replacement is placed in the corresponding
    /// Recommended field.
    ///
    /// MediaId always comes from SearchIssue.MediaId.
    /// </summary>
    private static List<SelectedMetadataExportRow>
        GetSelectedMetadataRows(
            IEnumerable<SearchIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var rows =
            new List<SelectedMetadataExportRow>();

        foreach (var issue in
                 issues.Where(
                     issue =>
                         issue is not null &&
                         string.Equals(
                             issue.Category,
                             "Metadata",
                             StringComparison.OrdinalIgnoreCase)))
        {
            var selectedRecommendations =
                issue.MetadataRecommendations
                    .Where(
                        recommendation =>
                            recommendation.IsSelected &&
                            recommendation.IsChange)
                    .ToList();

            if (selectedRecommendations.Count == 0)
                continue;

            var row =
                new SelectedMetadataExportRow
                {
                    IssueId =
                        issue.Id,

                    MediaId =
                        issue.MediaId,

                    FilePath =
                        issue.FilePath,

                    Artist =
                        issue.Artist,

                    ArtistRecommended =
                        issue.Artist,

                    Title =
                        issue.TrackTitle,

                    TitleRecommended =
                        issue.TrackTitle,

                    Album =
                        issue.Album,

                    AlbumRecommended =
                        issue.Album,

                    Genre =
                        issue.Genre,

                    GenreRecommended =
                        issue.Genre,

                    Year =
                        FormatNullable(issue.Year),

                    YearRecommended =
                        FormatNullable(issue.Year),

                    Bpm =
                        FormatNullable(issue.Bpm),

                    BpmRecommended =
                        FormatNullable(issue.Bpm),

                    Key =
                        issue.Key,

                    KeyRecommended =
                        issue.Key,

                    Duration =
                        FormatDuration(issue.Duration),

                    DurationRecommended =
                        FormatDuration(issue.Duration)
                };

            foreach (var recommendation in
                     selectedRecommendations)
            {
                ApplyRecommendation(
                    row,
                    recommendation);
            }

            rows.Add(row);
        }

        return rows;
    }

    // ============================================================
    // Apply Metadata Recommendation
    // ============================================================

    /// <summary>
    /// Applies one selected recommendation to the corresponding
    /// consolidated track-level export row.
    ///
    /// Only the selected replacement value is written into the
    /// appropriate Recommended field.
    ///
    /// The Current field remains the value held by the SearchIssue.
    /// </summary>
    private static void ApplyRecommendation(
        SelectedMetadataExportRow row,
        MetadataChangeRecommendation recommendation)
    {
        var field =
            recommendation.Field?.Trim();

        if (string.IsNullOrWhiteSpace(field))
            return;

        var recommendedValue =
            recommendation.RecommendedValue
            ?? string.Empty;

        switch (field.ToLowerInvariant())
        {
            case "artist":
                row.ArtistRecommended =
                    recommendedValue;
                break;

            case "title":
            case "track title":
            case "tracktitle":
                row.TitleRecommended =
                    recommendedValue;
                break;

            case "album":
                row.AlbumRecommended =
                    recommendedValue;
                break;

            case "genre":
                row.GenreRecommended =
                    recommendedValue;
                break;

            case "year":
                row.YearRecommended =
                    recommendedValue;
                break;

            case "bpm":
                row.BpmRecommended =
                    recommendedValue;
                break;

            case "key":
            case "musical key":
            case "musicalkey":
                row.KeyRecommended =
                    recommendedValue;
                break;

            case "duration":
                row.DurationRecommended =
                    recommendedValue;
                break;
        }
    }

    // ============================================================
    // Export Issue
    // ============================================================

    private static SearchExportIssue CreateExportIssue(
        SearchIssue issue)
    {
        return new SearchExportIssue
        {
            Id =
                issue.Id,

            MediaId =
                issue.MediaId,

            Category =
                issue.Category,

            Type =
                issue.Type,

            Title =
                issue.Title,

            Description =
                issue.Description,

            FilePath =
                issue.FilePath,

            Artist =
                issue.Artist,

            TrackTitle =
                issue.TrackTitle,

            Album =
                issue.Album,

            Genre =
                issue.Genre,

            Year =
                issue.Year,

            Bpm =
                issue.Bpm,

            Key =
                issue.Key,

            Duration =
                issue.Duration,

            MissingFields =
                issue.MissingFields
                    .ToArray(),

            SelectedResultIds =
                issue.SelectedResultIds
                    .ToArray(),

            Results =
                issue.Results
                    .Select(
                        CreateExportResult)
                    .ToArray(),

            SelectedMetadataChanges =
                issue.MetadataRecommendations
                    .Where(
                        recommendation =>
                            recommendation.IsSelected &&
                            recommendation.IsChange)
                    .Select(
                        CreateExportMetadataChange)
                    .ToArray()
        };
    }

    // ============================================================
    // Export Result
    // ============================================================

    private static SearchExportResult CreateExportResult(
        SearchResult result)
    {
        return new SearchExportResult
        {
            Id =
                result.Id,

            MediaId =
                result.MediaId,

            Source =
                result.Source,

            MatchScore =
                result.MatchScore,

            IsRecommended =
                result.IsRecommended,

            IsSelected =
                result.IsSelected,

            RecommendationReason =
                result.RecommendationReason,

            Artist =
                result.Artist,

            TrackTitle =
                result.TrackTitle,

            Album =
                result.Album,

            Genre =
                result.Genre,

            Bpm =
                result.Bpm,

            Key =
                result.Key,

            Duration =
                result.Duration,

            FilePath =
                result.FilePath,

            FileSize =
                result.FileSize,

            FileExists =
                result.FileExists,

            IsInspected =
                result.IsInspected,

            IsHealthy =
                result.IsHealthy,

            IntegrityStatus =
                result.IntegrityStatus,

            Format =
                result.Format,

            Codec =
                result.Codec,

            IsLossless =
                result.IsLossless,

            Bitrate =
                result.Bitrate,

            SampleRate =
                result.SampleRate,

            BitDepth =
                result.BitDepth,

            Channels =
                result.Channels
        };
    }

    // ============================================================
    // Export Metadata Change
    // ============================================================

    private static SearchExportMetadataChange
        CreateExportMetadataChange(
            MetadataChangeRecommendation recommendation)
    {
        return new SearchExportMetadataChange
        {
            Field =
                recommendation.Field,

            CurrentValue =
                recommendation.CurrentValue,

            RecommendedValue =
                recommendation.RecommendedValue,

            AgreementPercentage =
                recommendation.AgreementPercentage,

            SupportingProviders =
                recommendation.SupportingProviders,

            ProvidersWithValue =
                recommendation.ProvidersWithValue,

            Strength =
                recommendation.Strength.ToString(),

            IsRecommended =
                recommendation.IsRecommended,

            IsSelected =
                recommendation.IsSelected,

            IsUserModified =
                recommendation.IsUserModified,

            Reason =
                recommendation.Reason
        };
    }

    // ============================================================
    // General CSV
    // ============================================================

    private static void WriteCsvHeader(
        StringBuilder builder)
    {
        var headers =
            new[]
            {
                "Issue ID",
                "DIASISS Media ID",
                "Category",
                "Issue Type",
                "Issue Title",
                "Description",
                "File Path",
                "Artist",
                "Title",
                "Album",
                "Genre",
                "Year",
                "BPM",
                "Key",
                "Duration",
                "Missing Fields",

                "Result ID",
                "Result Media ID",
                "Result Source",
                "Match Score",
                "Recommended Result",
                "Selected Result",
                "Recommendation Reason",

                "Result Artist",
                "Result Title",
                "Result Album",
                "Result Genre",
                "Result BPM",
                "Result Key",
                "Result Duration",
                "Result File Path",
                "Result File Size",
                "File Exists",
                "Inspected",
                "Healthy",
                "Integrity Status",
                "Format",
                "Codec",
                "Lossless",
                "Bitrate",
                "Sample Rate",
                "Bit Depth",
                "Channels",

                "Metadata Field",
                "Current Value",
                "Recommended Value",
                "Agreement %",
                "Supporting Providers",
                "Providers With Value",
                "Consensus Strength",
                "Recommended Change",
                "Selected Change",
                "User Modified",
                "Metadata Reason"
            };

        builder.AppendLine(
            string.Join(
                ",",
                headers.Select(
                    EscapeCsv)));
    }

    private static void WriteIssueCsvRows(
        StringBuilder builder,
        SearchIssue issue)
    {
        var results =
            issue.Results
                .Where(
                    result =>
                        result.IsSelected ||
                        result.IsRecommended)
                .ToList();

        var metadataChanges =
            issue.MetadataRecommendations
                .Where(
                    recommendation =>
                        recommendation.IsSelected &&
                        recommendation.IsChange)
                .ToList();

        if (results.Count == 0 &&
            metadataChanges.Count == 0)
        {
            WriteCsvRow(
                builder,
                issue,
                null,
                null);

            return;
        }

        foreach (var result in results)
        {
            WriteCsvRow(
                builder,
                issue,
                result,
                null);
        }

        foreach (var recommendation in metadataChanges)
        {
            WriteCsvRow(
                builder,
                issue,
                null,
                recommendation);
        }
    }

    private static void WriteCsvRow(
        StringBuilder builder,
        SearchIssue issue,
        SearchResult? result,
        MetadataChangeRecommendation? recommendation)
    {
        var values =
            new[]
            {
                issue.Id,
                issue.MediaId,
                issue.Category,
                issue.Type,
                issue.Title,
                issue.Description,
                issue.FilePath,
                issue.Artist,
                issue.TrackTitle,
                issue.Album,
                issue.Genre,
                FormatNullable(issue.Year),
                FormatNullable(issue.Bpm),
                issue.Key,
                FormatDuration(issue.Duration),
                string.Join(
                    "; ",
                    issue.MissingFields),

                result?.Id ?? string.Empty,
                result?.MediaId ?? issue.MediaId ?? string.Empty,
                result?.Source ?? string.Empty,
                result is null
                    ? string.Empty
                    : result.MatchScore.ToString(
                        "F1",
                        CultureInfo.InvariantCulture),
                result is null
                    ? string.Empty
                    : result.IsRecommended.ToString(),
                result is null
                    ? string.Empty
                    : result.IsSelected.ToString(),
                result?.RecommendationReason
                    ?? string.Empty,

                result?.Artist ?? string.Empty,
                result?.TrackTitle ?? string.Empty,
                result?.Album ?? string.Empty,
                result?.Genre ?? string.Empty,
                result is null
                    ? string.Empty
                    : FormatNullable(result.Bpm),
                result?.Key ?? string.Empty,
                result is null
                    ? string.Empty
                    : FormatDuration(result.Duration),
                result?.FilePath ?? string.Empty,
                result is null
                    ? string.Empty
                    : FormatNullable(result.FileSize),
                result is null
                    ? string.Empty
                    : result.FileExists.ToString(),
                result is null
                    ? string.Empty
                    : FormatNullable(result.IsInspected),
                result is null
                    ? string.Empty
                    : FormatNullable(result.IsHealthy),
                result?.IntegrityStatus
                    ?? string.Empty,
                result?.Format ?? string.Empty,
                result?.Codec ?? string.Empty,
                result is null
                    ? string.Empty
                    : FormatNullable(result.IsLossless),
                result is null
                    ? string.Empty
                    : FormatNullable(result.Bitrate),
                result is null
                    ? string.Empty
                    : FormatNullable(result.SampleRate),
                result is null
                    ? string.Empty
                    : FormatNullable(result.BitDepth),
                result is null
                    ? string.Empty
                    : FormatNullable(result.Channels),

                recommendation?.Field
                    ?? string.Empty,
                recommendation?.CurrentValue
                    ?? string.Empty,
                recommendation?.RecommendedValue
                    ?? string.Empty,
                recommendation is null
                    ? string.Empty
                    : recommendation.AgreementPercentage
                        .ToString(
                            "F1",
                            CultureInfo.InvariantCulture),
                recommendation is null
                    ? string.Empty
                    : recommendation.SupportingProviders
                        .ToString(
                            CultureInfo.InvariantCulture),
                recommendation is null
                    ? string.Empty
                    : recommendation.ProvidersWithValue
                        .ToString(
                            CultureInfo.InvariantCulture),
                recommendation?.Strength
                    .ToString()
                    ?? string.Empty,
                recommendation is null
                    ? string.Empty
                    : recommendation.IsRecommended.ToString(),
                recommendation is null
                    ? string.Empty
                    : recommendation.IsSelected.ToString(),
                recommendation is null
                    ? string.Empty
                    : recommendation.IsUserModified.ToString(),
                recommendation?.Reason
                    ?? string.Empty
            };

        builder.AppendLine(
            string.Join(
                ",",
                values.Select(
                    EscapeCsv)));
    }

    // ============================================================
    // Selected Metadata CSV
    // ============================================================

    private static void WriteSelectedMetadataCsvHeader(
        StringBuilder builder)
    {
        var headers =
            new[]
            {
                "Issue ID",
                "DIASISS Media ID",
                "File Path",

                "Artist",
                "Artist Recommended",

                "Title",
                "Title Recommended",

                "Album",
                "Album Recommended",

                "Genre",
                "Genre Recommended",

                "Year",
                "Year Recommended",

                "BPM",
                "BPM Recommended",

                "Key",
                "Key Recommended",

                "Duration",
                "Duration Recommended"
            };

        builder.AppendLine(
            string.Join(
                ",",
                headers.Select(
                    EscapeCsv)));
    }

    private static void WriteSelectedMetadataCsvRow(
        StringBuilder builder,
        SelectedMetadataExportRow row)
    {
        var values =
            new[]
            {
                row.IssueId,
                row.MediaId,
                row.FilePath,

                row.Artist,
                row.ArtistRecommended,

                row.Title,
                row.TitleRecommended,

                row.Album,
                row.AlbumRecommended,

                row.Genre,
                row.GenreRecommended,

                row.Year,
                row.YearRecommended,

                row.Bpm,
                row.BpmRecommended,

                row.Key,
                row.KeyRecommended,

                row.Duration,
                row.DurationRecommended
            };

        builder.AppendLine(
            string.Join(
                ",",
                values.Select(
                    EscapeCsv)));
    }

    // ============================================================
    // General XLSX
    // ============================================================

    private static void WriteXlsxHeader(
        IXLWorksheet worksheet)
    {
        var headers =
            new[]
            {
                "Issue ID",
                "DIASISS Media ID",
                "Category",
                "Issue Type",
                "Issue Title",
                "Description",
                "File Path",
                "Artist",
                "Title",
                "Album",
                "Genre",
                "Year",
                "BPM",
                "Key",
                "Duration",
                "Missing Fields",

                "Result ID",
                "Result Media ID",
                "Result Source",
                "Match Score",
                "Recommended Result",
                "Selected Result",
                "Recommendation Reason",

                "Result Artist",
                "Result Title",
                "Result Album",
                "Result Genre",
                "Result BPM",
                "Result Key",
                "Result Duration",
                "Result File Path",
                "Result File Size",
                "File Exists",
                "Inspected",
                "Healthy",
                "Integrity Status",
                "Format",
                "Codec",
                "Lossless",
                "Bitrate",
                "Sample Rate",
                "Bit Depth",
                "Channels",

                "Metadata Field",
                "Current Value",
                "Recommended Value",
                "Agreement %",
                "Supporting Providers",
                "Providers With Value",
                "Consensus Strength",
                "Recommended Change",
                "Selected Change",
                "User Modified",
                "Metadata Reason"
            };

        for (var index = 0;
             index < headers.Length;
             index++)
        {
            worksheet.Cell(
                    1,
                    index + 1)
                .Value =
                headers[index];
        }
    }

    private static int WriteIssueXlsxRows(
        IXLWorksheet worksheet,
        int row,
        SearchIssue issue)
    {
        var results =
            issue.Results
                .Where(
                    result =>
                        result.IsSelected ||
                        result.IsRecommended)
                .ToList();

        var metadataChanges =
            issue.MetadataRecommendations
                .Where(
                    recommendation =>
                        recommendation.IsSelected &&
                        recommendation.IsChange)
                .ToList();

        if (results.Count == 0 &&
            metadataChanges.Count == 0)
        {
            WriteXlsxRow(
                worksheet,
                row,
                issue,
                null,
                null);

            return row + 1;
        }

        foreach (var result in results)
        {
            WriteXlsxRow(
                worksheet,
                row,
                issue,
                result,
                null);

            row++;
        }

        foreach (var recommendation in metadataChanges)
        {
            WriteXlsxRow(
                worksheet,
                row,
                issue,
                null,
                recommendation);

            row++;
        }

        return row;
    }

    private static void WriteXlsxRow(
        IXLWorksheet worksheet,
        int row,
        SearchIssue issue,
        SearchResult? result,
        MetadataChangeRecommendation? recommendation)
    {
        var values =
            new object?[]
            {
                issue.Id,
                issue.MediaId,
                issue.Category,
                issue.Type,
                issue.Title,
                issue.Description,
                issue.FilePath,
                issue.Artist,
                issue.TrackTitle,
                issue.Album,
                issue.Genre,
                issue.Year,
                issue.Bpm,
                issue.Key,
                FormatDuration(issue.Duration),
                string.Join(
                    "; ",
                    issue.MissingFields),

                result?.Id,
                result?.MediaId ?? issue.MediaId,
                result?.Source,
                result?.MatchScore,
                result?.IsRecommended,
                result?.IsSelected,
                result?.RecommendationReason,

                result?.Artist,
                result?.TrackTitle,
                result?.Album,
                result?.Genre,
                result?.Bpm,
                result?.Key,
                FormatDuration(result?.Duration),
                result?.FilePath,
                result?.FileSize,
                result?.FileExists,
                result?.IsInspected,
                result?.IsHealthy,
                result?.IntegrityStatus,
                result?.Format,
                result?.Codec,
                result?.IsLossless,
                result?.Bitrate,
                result?.SampleRate,
                result?.BitDepth,
                result?.Channels,

                recommendation?.Field,
                recommendation?.CurrentValue,
                recommendation?.RecommendedValue,
                recommendation?.AgreementPercentage,
                recommendation?.SupportingProviders,
                recommendation?.ProvidersWithValue,
                recommendation?.Strength.ToString(),
                recommendation?.IsRecommended,
                recommendation?.IsSelected,
                recommendation?.IsUserModified,
                recommendation?.Reason
            };

        for (var index = 0;
             index < values.Length;
             index++)
        {
            worksheet.Cell(
                    row,
                    index + 1)
                .Value =
                XLCellValue.FromObject(
                    values[index]);
        }
    }

    // ============================================================
    // Selected Metadata XLSX
    // ============================================================

    private static void WriteSelectedMetadataXlsxHeader(
        IXLWorksheet worksheet)
    {
        var headers =
            new[]
            {
                "Issue ID",
                "DIASISS Media ID",
                "File Path",

                "Artist",
                "Artist Recommended",

                "Title",
                "Title Recommended",

                "Album",
                "Album Recommended",

                "Genre",
                "Genre Recommended",

                "Year",
                "Year Recommended",

                "BPM",
                "BPM Recommended",

                "Key",
                "Key Recommended",

                "Duration",
                "Duration Recommended"
            };

        for (var index = 0;
             index < headers.Length;
             index++)
        {
            worksheet.Cell(
                    1,
                    index + 1)
                .Value =
                headers[index];
        }
    }

    private static void WriteSelectedMetadataXlsxRow(
        IXLWorksheet worksheet,
        int rowNumber,
        SelectedMetadataExportRow row)
    {
        var values =
            new object?[]
            {
                row.IssueId,
                row.MediaId,
                row.FilePath,

                row.Artist,
                row.ArtistRecommended,

                row.Title,
                row.TitleRecommended,

                row.Album,
                row.AlbumRecommended,

                row.Genre,
                row.GenreRecommended,

                row.Year,
                row.YearRecommended,

                row.Bpm,
                row.BpmRecommended,

                row.Key,
                row.KeyRecommended,

                row.Duration,
                row.DurationRecommended
            };

        for (var index = 0;
             index < values.Length;
             index++)
        {
            worksheet.Cell(
                    rowNumber,
                    index + 1)
                .Value =
                XLCellValue.FromObject(
                    values[index]);
        }
    }

    // ============================================================
    // XLSX Formatting
    // ============================================================

    private static void FormatWorksheet(
        IXLWorksheet worksheet)
    {
        var usedRange =
            worksheet.RangeUsed();

        if (usedRange is null)
            return;

        usedRange
            .SetAutoFilter();

        worksheet.SheetView
            .FreezeRows(1);

        worksheet
            .Row(1)
            .Style
            .Font
            .Bold = true;

        worksheet
            .Columns()
            .AdjustToContents();

        foreach (var column in worksheet.Columns())
        {
            if (column.Width > 60)
                column.Width = 60;
        }
    }

    // ============================================================
    // Formatting Helpers
    // ============================================================

    private static string EscapeCsv(
        string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        var escaped =
            value.Replace(
                "\"",
                "\"\"",
                StringComparison.Ordinal);

        return $"\"{escaped}\"";
    }

    private static string FormatNullable<T>(
        T? value)
        where T : struct
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static string FormatDuration(
        TimeSpan? duration)
    {
        return duration.HasValue
            ? duration.Value.ToString(
                @"hh\:mm\:ss",
                CultureInfo.InvariantCulture)
            : string.Empty;
    }

    // ============================================================
    // JSON Options
    // ============================================================

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true,

            DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull
        };

    // ============================================================
    // JSON Export Models
    // ============================================================

    private sealed class SearchExportIssue
    {
        public string Id { get; init; } = string.Empty;

        public string MediaId { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string FilePath { get; init; } = string.Empty;

        public string Artist { get; init; } = string.Empty;

        public string TrackTitle { get; init; } = string.Empty;

        public string Album { get; init; } = string.Empty;

        public string Genre { get; init; } = string.Empty;

        public int? Year { get; init; }

        public double? Bpm { get; init; }

        public string Key { get; init; } = string.Empty;

        public TimeSpan? Duration { get; init; }

        public IReadOnlyList<string> MissingFields { get; init; } =
            Array.Empty<string>();

        public IReadOnlyList<string> SelectedResultIds { get; init; } =
            Array.Empty<string>();

        public IReadOnlyList<SearchExportResult> Results { get; init; } =
            Array.Empty<SearchExportResult>();

        public IReadOnlyList<SearchExportMetadataChange>
            SelectedMetadataChanges
        { get; init; } =
            Array.Empty<SearchExportMetadataChange>();
    }

    private sealed class SearchExportResult
    {
        public string Id { get; init; } = string.Empty;

        public string MediaId { get; init; } = string.Empty;

        public string Source { get; init; } = string.Empty;

        public double MatchScore { get; init; }

        public bool IsRecommended { get; init; }

        public bool IsSelected { get; init; }

        public string RecommendationReason { get; init; } = string.Empty;

        public string Artist { get; init; } = string.Empty;

        public string TrackTitle { get; init; } = string.Empty;

        public string Album { get; init; } = string.Empty;

        public string Genre { get; init; } = string.Empty;

        public double? Bpm { get; init; }

        public string Key { get; init; } = string.Empty;

        public TimeSpan? Duration { get; init; }

        public string FilePath { get; init; } = string.Empty;

        public long? FileSize { get; init; }

        public bool FileExists { get; init; }

        public bool? IsInspected { get; init; }

        public bool? IsHealthy { get; init; }

        public string IntegrityStatus { get; init; } = string.Empty;

        public string Format { get; init; } = string.Empty;

        public string Codec { get; init; } = string.Empty;

        public bool? IsLossless { get; init; }

        public int? Bitrate { get; init; }

        public int? SampleRate { get; init; }

        public int? BitDepth { get; init; }

        public int? Channels { get; init; }
    }

    private sealed class SearchExportMetadataChange
    {
        public string Field { get; init; } = string.Empty;

        public string CurrentValue { get; init; } = string.Empty;

        public string RecommendedValue { get; init; } = string.Empty;

        public double AgreementPercentage { get; init; }

        public int SupportingProviders { get; init; }

        public int ProvidersWithValue { get; init; }

        public string Strength { get; init; } = string.Empty;

        public bool IsRecommended { get; init; }

        public bool IsSelected { get; init; }

        public bool IsUserModified { get; init; }

        public string Reason { get; init; } = string.Empty;
    }

    // ============================================================
    // Selected Metadata Export Model
    // ============================================================

    /// <summary>
    /// Track-level metadata export row.
    ///
    /// One instance represents exactly one track / SearchIssue.
    ///
    /// Multiple selected metadata changes are consolidated into
    /// the corresponding Recommended fields.
    /// </summary>
    private sealed class SelectedMetadataExportRow
    {
        public string IssueId { get; init; } = string.Empty;

        public string MediaId { get; init; } = string.Empty;

        public string FilePath { get; init; } = string.Empty;

        public string Artist { get; init; } = string.Empty;

        public string ArtistRecommended { get; set; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string TitleRecommended { get; set; } = string.Empty;

        public string Album { get; init; } = string.Empty;

        public string AlbumRecommended { get; set; } = string.Empty;

        public string Genre { get; init; } = string.Empty;

        public string GenreRecommended { get; set; } = string.Empty;

        public string Year { get; init; } = string.Empty;

        public string YearRecommended { get; set; } = string.Empty;

        public string Bpm { get; init; } = string.Empty;

        public string BpmRecommended { get; set; } = string.Empty;

        public string Key { get; init; } = string.Empty;

        public string KeyRecommended { get; set; } = string.Empty;

        public string Duration { get; init; } = string.Empty;

        public string DurationRecommended { get; set; } = string.Empty;
    }
}