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
/// This service identifies candidate recordings only.
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
            {
                continue;
            }

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
    /// Returns candidates that represent a sufficiently strong
    /// recording match.
    ///
    /// Duration is only used as a rejection criterion when BOTH
    /// the local track and the provider candidate have a valid
    /// duration.
    ///
    /// A missing or zero local duration means that duration is
    /// unknown and therefore cannot be used to reject a candidate.
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
    /// Returns candidates that represent a sufficiently strong
    /// recording match, optionally using a filename-derived
    /// search hypothesis.
    ///
    /// Duration matching is conditional:
    ///
    /// - If the local duration is unavailable, duration does not
    ///   affect candidate viability.
    ///
    /// - If the local duration is available but the provider
    ///   duration is unavailable, duration does not affect
    ///   candidate viability.
    ///
    /// - If BOTH durations are available, a zero DurationScore
    ///   means the recordings differ too substantially and the
    ///   candidate is rejected.
    /// </summary>
    public IReadOnlyList<MetadataEvidenceAnalysisResult>
        GetViableCandidates(
            DJLMMediaItem media,
            IEnumerable<MetadataEvidence> evidence,
            FilenameSearchHint? filenameSearchHint)
    {
        var analysed =
            Analyse(
                media,
                evidence,
                filenameSearchHint);

        var localDurationAvailable =
            media.Duration.HasValue &&
            media.Duration.Value.TotalSeconds > 0;

        return analysed
            .Where(
                candidate =>
                {
                    if (!candidate.Match.IsMatch)
                    {
                        return false;
                    }

                    //
                    // No usable local duration.
                    //
                    // Duration cannot be used to reject the
                    // provider candidate.
                    //
                    if (!localDurationAvailable)
                    {
                        return true;
                    }

                    //
                    // Local duration exists, but provider has
                    // no usable duration.
                    //
                    // Again, duration cannot be used to reject.
                    //
                    var candidateDuration =
                        candidate.Evidence.Duration;

                    var candidateDurationAvailable =
                        candidateDuration.HasValue &&
                        candidateDuration.Value.TotalSeconds > 0;

                    if (!candidateDurationAvailable)
                    {
                        return true;
                    }

                    //
                    // Both durations exist.
                    //
                    // Now DurationScore is meaningful and a score
                    // of zero means the recordings differ too much.
                    //
                    return candidate.Match.DurationScore > 0;
                })
            .OrderByDescending(
                candidate =>
                    candidate.Match.Score)
            .ToList();
    }
}