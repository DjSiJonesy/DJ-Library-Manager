using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Analyses independently collected metadata evidence against
/// a local DIASISS media item.
///
/// This service does not pass information between providers.
/// Every MetadataEvidence item is evaluated independently against
/// the original local track.
///
/// FilenameSearchHint may optionally be supplied when confirmed
/// Artist and/or Title metadata is missing. Filename information
/// is treated only as a search hypothesis and is never written
/// into the local media item.
///
/// This service identifies viable candidate recordings only.
/// It does not resolve conflicting metadata or modify the
/// DIASISS library.
/// </summary>
public sealed class MetadataEvidenceAnalysisService
{
    private readonly MetadataCandidateMatcher _candidateMatcher;

    public MetadataEvidenceAnalysisService(
        MetadataCandidateMatcher? candidateMatcher = null)
    {
        _candidateMatcher =
            candidateMatcher ??
            new MetadataCandidateMatcher();
    }

    // ============================================================
    // Analyse
    // ============================================================

    /// <summary>
    /// Evaluates all supplied metadata evidence independently
    /// against the local library track.
    ///
    /// This overload preserves the original behaviour for callers
    /// that do not have filename search hints.
    /// </summary>
    public IReadOnlyList<MetadataEvidenceAnalysisResult> Analyse(
        DJLMMediaItem media,
        IEnumerable<MetadataEvidence> evidence)
    {
        return Analyse(
            media,
            evidence,
            filenameSearchHint: null);
    }

    /// <summary>
    /// Evaluates all supplied metadata evidence independently
    /// against the local library track.
    ///
    /// When Artist and/or Title are missing from the local library,
    /// the optional filename search hint is supplied to the
    /// candidate matcher as a search hypothesis.
    ///
    /// The filename-derived values are never copied into the
    /// DJLMMediaItem.
    /// </summary>
    public IReadOnlyList<MetadataEvidenceAnalysisResult> Analyse(
        DJLMMediaItem media,
        IEnumerable<MetadataEvidence> evidence,
        FilenameSearchHint? filenameSearchHint)
    {
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(evidence);

        var results =
            new List<MetadataEvidenceAnalysisResult>();

        foreach (var item in evidence)
        {
            if (item is null)
                continue;

            var match =
                _candidateMatcher.Match(
                    media,
                    item,
                    filenameSearchHint);

            results.Add(
                new MetadataEvidenceAnalysisResult
                {
                    Evidence =
                        item,

                    Match =
                        match
                });
        }

        return results;
    }

    // ============================================================
    // Viable Candidates
    // ============================================================

    /// <summary>
    /// Returns only evidence candidates that the candidate
    /// matcher considers viable matches.
    ///
    /// This overload preserves the original behaviour.
    /// </summary>
    public IReadOnlyList<MetadataEvidenceAnalysisResult>
        GetViableCandidates(
            DJLMMediaItem media,
            IEnumerable<MetadataEvidence> evidence)
    {
        return GetViableCandidates(
            media,
            evidence,
            filenameSearchHint: null);
    }

    /// <summary>
    /// Returns only evidence candidates that the candidate
    /// matcher considers viable matches, optionally using a
    /// filename-derived search hypothesis.
    /// </summary>
    public IReadOnlyList<MetadataEvidenceAnalysisResult>
        GetViableCandidates(
            DJLMMediaItem media,
            IEnumerable<MetadataEvidence> evidence,
            FilenameSearchHint? filenameSearchHint)
    {
        return Analyse(
                media,
                evidence,
                filenameSearchHint)
            .Where(
                x => x.Match.IsMatch)
            .OrderByDescending(
                x => x.Match.Score)
            .ToList();
    }
}