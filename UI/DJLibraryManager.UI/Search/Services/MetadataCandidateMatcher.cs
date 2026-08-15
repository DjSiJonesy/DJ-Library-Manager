using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Compares an external metadata candidate against a local
/// DIASISS media item.
///
/// The matcher evaluates:
///
/// Artist
/// Title
/// Duration
/// BPM
///
/// Key is deliberately excluded from candidate identity matching
/// because different audio-analysis engines can legitimately
/// produce different key results.
///
/// Missing metadata is neutral evidence.
///
/// Conflicting metadata is negative evidence.
///
/// Filename search hints may optionally be supplied when Artist
/// and/or Title are missing from the local library. Filename hints
/// are treated only as search hypotheses. They are never treated
/// as confirmed library metadata.
///
/// The matcher does not modify the library and does not make the
/// final metadata recommendation.
/// </summary>
public sealed class MetadataCandidateMatcher
{
    // ============================================================
    // Configuration
    // ============================================================

    private const double MatchThreshold = 85.0;

    private const double ArtistWeight = 35.0;

    private const double TitleWeight = 35.0;

    private const double DurationWeight = 15.0;

    private const double BPMWeight = 15.0;

    /// <summary>
    /// Maximum proportional duration difference allowed for a
    /// candidate to be considered the same recording.
    ///
    /// A candidate beyond this point may still represent the
    /// same musical work, such as an extended or 12" version,
    /// but it is not treated as the same recording.
    /// </summary>
    private const double MaximumRecordingDurationDifference = 0.20;

    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// Compares a metadata evidence candidate against a local
    /// library track using only confirmed library metadata.
    ///
    /// This is the original matching behaviour.
    /// </summary>
    public MetadataCandidateMatch Match(
        DJLMMediaItem media,
        MetadataEvidence evidence)
    {
        return Match(
            media,
            evidence,
            filenameSearchHint: null);
    }

    /// <summary>
    /// Compares a metadata evidence candidate against a local
    /// library track.
    ///
    /// When filename search hints are supplied, they are used only
    /// as search hypotheses for missing Artist and/or Title values.
    ///
    /// Confirmed library metadata always takes precedence over
    /// filename-derived values.
    ///
    /// The filename hypothesis is never written back to the local
    /// media object and is never treated as confirmed metadata.
    /// </summary>
    public MetadataCandidateMatch Match(
        DJLMMediaItem media,
        MetadataEvidence evidence,
        FilenameSearchHint? filenameSearchHint)
    {
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(evidence);

        // --------------------------------------------------------
        // If there is no filename hint, use the normal matcher.
        // --------------------------------------------------------

        if (filenameSearchHint is null ||
            filenameSearchHint.Candidates.Count == 0)
        {
            return MatchCore(
                media,
                evidence,
                media.Artist,
                media.Title,
                filenameHypothesisUsed: false);
        }

        // --------------------------------------------------------
        // Build the best possible identity hypothesis.
        //
        // Confirmed local metadata is always retained.
        //
        // Missing local Artist/Title values may be supplied by
        // one of the filename candidates.
        // --------------------------------------------------------

        var hypotheses =
            BuildIdentityHypotheses(
                media,
                filenameSearchHint);

        if (hypotheses.Count == 0)
        {
            return MatchCore(
                media,
                evidence,
                media.Artist,
                media.Title,
                filenameHypothesisUsed: false);
        }

        MetadataCandidateMatch? bestMatch = null;

        foreach (var hypothesis in hypotheses)
        {
            var match =
                MatchCore(
                    media,
                    evidence,
                    hypothesis.Artist,
                    hypothesis.Title,
                    hypothesis.UsedFilenameHypothesis);

            if (bestMatch is null ||
                match.Score > bestMatch.Score)
            {
                bestMatch = match;
            }
        }

        return bestMatch
            ?? MatchCore(
                media,
                evidence,
                media.Artist,
                media.Title,
                filenameHypothesisUsed: false);
    }

    // ============================================================
    // Filename Identity Hypotheses
    // ============================================================

    /// <summary>
    /// Builds identity hypotheses from the filename search hint.
    ///
    /// Existing confirmed Artist and Title values are preserved.
    /// Filename values are used only where the corresponding
    /// confirmed value is missing.
    /// </summary>
    private static List<IdentityHypothesis>
        BuildIdentityHypotheses(
            DJLMMediaItem media,
            FilenameSearchHint filenameSearchHint)
    {
        var hypotheses =
            new List<IdentityHypothesis>();

        var localArtist =
            media.Artist?.Trim()
            ?? string.Empty;

        var localTitle =
            media.Title?.Trim()
            ?? string.Empty;

        foreach (var candidate in
                 filenameSearchHint.Candidates)
        {
            if (candidate is null)
            {
                continue;
            }

            var artist =
                !string.IsNullOrWhiteSpace(localArtist)
                    ? localArtist
                    : candidate.Artist?.Trim()
                        ?? string.Empty;

            var title =
                !string.IsNullOrWhiteSpace(localTitle)
                    ? localTitle
                    : candidate.Title?.Trim()
                        ?? string.Empty;

            if (string.IsNullOrWhiteSpace(artist) &&
                string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var usedFilenameHypothesis =
                string.IsNullOrWhiteSpace(localArtist) &&
                !string.IsNullOrWhiteSpace(
                    candidate.Artist)
                ||
                string.IsNullOrWhiteSpace(localTitle) &&
                !string.IsNullOrWhiteSpace(
                    candidate.Title);

            var duplicate =
                hypotheses.Any(
                    existing =>
                        string.Equals(
                            existing.Artist,
                            artist,
                            StringComparison.OrdinalIgnoreCase)
                        &&
                        string.Equals(
                            existing.Title,
                            title,
                            StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                continue;
            }

            hypotheses.Add(
                new IdentityHypothesis(
                    artist,
                    title,
                    usedFilenameHypothesis));
        }

        return hypotheses;
    }

    // ============================================================
    // Core Matching
    // ============================================================

    private static MetadataCandidateMatch MatchCore(
        DJLMMediaItem media,
        MetadataEvidence evidence,
        string? identityArtist,
        string? identityTitle,
        bool filenameHypothesisUsed)
    {
        var artistScore =
            CalculateArtistScore(
                identityArtist,
                evidence.Artist);

        var titleScore =
            CalculateTitleScore(
                identityTitle,
                evidence.Title);

        var durationResult =
            CalculateDurationScore(
                media.Duration,
                evidence.Duration);

        var bpmResult =
            CalculateBPMScore(
                media.BPM,
                evidence.BPM);

        var weightedScore = 0.0;
        var availableWeight = 0.0;

        //
        // Missing fields are neutral.
        //
        // Fields which exist on both sides are included in the
        // calculation, including mismatches.
        //

        AddWeightedScore(
            ref weightedScore,
            ref availableWeight,
            artistScore,
            ArtistWeight,
            !string.IsNullOrWhiteSpace(identityArtist) &&
            !string.IsNullOrWhiteSpace(evidence.Artist));

        AddWeightedScore(
            ref weightedScore,
            ref availableWeight,
            titleScore,
            TitleWeight,
            !string.IsNullOrWhiteSpace(identityTitle) &&
            !string.IsNullOrWhiteSpace(evidence.Title));

        AddWeightedScore(
            ref weightedScore,
            ref availableWeight,
            durationResult.Score,
            DurationWeight,
            media.Duration.HasValue &&
            evidence.Duration.HasValue);

        AddWeightedScore(
            ref weightedScore,
            ref availableWeight,
            bpmResult.Score,
            BPMWeight,
            media.BPM.HasValue &&
            evidence.BPM.HasValue);

        var overallScore =
            availableWeight > 0
                ? weightedScore / availableWeight
                : 0;

        //
        // Artist and Title remain the primary identity signals.
        //
        // When a filename hypothesis is being used, those values
        // represent a search hypothesis rather than confirmed
        // library metadata.
        //

        var primaryIdentityMatch =
            artistScore >= 70 &&
            titleScore >= 70;

        //
        // A very large duration difference is strong evidence that
        // the provider has returned a different recording/version.
        //
        // We deliberately apply this as a veto rather than allowing
        // Artist + Title + BPM to rescue the candidate.
        //

        var severeDurationConflict =
            durationResult.Available &&
            durationResult.ProportionalDifference >
                MaximumRecordingDurationDifference;

        var isMatch =
            primaryIdentityMatch &&
            overallScore >= MatchThreshold &&
            !severeDurationConflict;

        var reason =
            BuildReason(
                artistScore,
                titleScore,
                durationResult.Score,
                durationResult.ProportionalDifference,
                durationResult.Available,
                bpmResult.Score,
                bpmResult.HalfDoubleMatch,
                media.Duration,
                evidence.Duration,
                media.BPM,
                evidence.BPM,
                severeDurationConflict,
                filenameHypothesisUsed,
                isMatch);

        return new MetadataCandidateMatch
        {
            Score =
                Math.Round(
                    Math.Clamp(
                        overallScore,
                        0,
                        100),
                    1),

            IsMatch =
                isMatch,

            ArtistScore =
                Math.Round(
                    artistScore,
                    1),

            TitleScore =
                Math.Round(
                    titleScore,
                    1),

            DurationScore =
                Math.Round(
                    durationResult.Score,
                    1),

            BPMScore =
                Math.Round(
                    bpmResult.Score,
                    1),

            BPMHalfDoubleMatch =
                bpmResult.HalfDoubleMatch,

            Reason =
                reason
        };
    }

    // ============================================================
    // Artist
    // ============================================================

    private static double CalculateArtistScore(
        string? localArtist,
        string? candidateArtist)
    {
        if (string.IsNullOrWhiteSpace(localArtist) ||
            string.IsNullOrWhiteSpace(candidateArtist))
        {
            return 0;
        }

        var local =
            NormaliseArtist(localArtist);

        var candidate =
            NormaliseArtist(candidateArtist);

        if (local.Equals(
                candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        //
        // Providers may include featured artists while the local
        // library contains only the primary artist.
        //

        var localParts =
            SplitArtistNames(local);

        var candidateParts =
            SplitArtistNames(candidate);

        if (localParts.Count > 0 &&
            candidateParts.Count > 0)
        {
            var matched =
                localParts.Count(
                    localName =>
                        candidateParts.Any(
                            candidateName =>
                                NamesEquivalent(
                                    localName,
                                    candidateName)));

            if (matched == localParts.Count)
            {
                return 95;
            }
        }

        if (candidate.Contains(
                local,
                StringComparison.OrdinalIgnoreCase) ||
            local.Contains(
                candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            return 85;
        }

        return CalculateTokenSimilarity(
            local,
            candidate);
    }

    // ============================================================
    // Title
    // ============================================================

    private static double CalculateTitleScore(
        string? localTitle,
        string? candidateTitle)
    {
        if (string.IsNullOrWhiteSpace(localTitle) ||
            string.IsNullOrWhiteSpace(candidateTitle))
        {
            return 0;
        }

        var local =
            NormaliseTitle(localTitle);

        var candidate =
            NormaliseTitle(candidateTitle);

        if (local.Equals(
                candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        //
        // A provider may append version information or featured
        // artist information.
        //

        if (candidate.StartsWith(
                local + " ",
                StringComparison.OrdinalIgnoreCase))
        {
            return 95;
        }

        if (candidate.StartsWith(
                local + "(",
                StringComparison.OrdinalIgnoreCase))
        {
            return 95;
        }

        if (local.StartsWith(
                candidate + " ",
                StringComparison.OrdinalIgnoreCase) ||
            local.StartsWith(
                candidate + "(",
                StringComparison.OrdinalIgnoreCase))
        {
            return 90;
        }

        if (candidate.Contains(
                local,
                StringComparison.OrdinalIgnoreCase) ||
            local.Contains(
                candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            return 85;
        }

        return CalculateTokenSimilarity(
            local,
            candidate);
    }

    // ============================================================
    // Duration
    // ============================================================

    private static DurationMatchResult CalculateDurationScore(
        TimeSpan? localDuration,
        TimeSpan? candidateDuration)
    {
        if (!localDuration.HasValue ||
            !candidateDuration.HasValue)
        {
            return new DurationMatchResult(
                Score: 0,
                ProportionalDifference: 0,
                Available: false);
        }

        if (localDuration.Value.TotalSeconds <= 0 ||
            candidateDuration.Value.TotalSeconds <= 0)
        {
            return new DurationMatchResult(
                Score: 0,
                ProportionalDifference: 0,
                Available: true);
        }

        var localSeconds =
            localDuration.Value.TotalSeconds;

        var candidateSeconds =
            candidateDuration.Value.TotalSeconds;

        var absoluteDifference =
            Math.Abs(
                localSeconds -
                candidateSeconds);

        //
        // Proportional difference is measured against the local
        // track duration.
        //

        var proportionalDifference =
            absoluteDifference /
            localSeconds;

        if (absoluteDifference <= 2)
        {
            return new DurationMatchResult(
                Score: 100,
                ProportionalDifference:
                    proportionalDifference,
                Available: true);
        }

        if (proportionalDifference <= 0.02)
        {
            return new DurationMatchResult(
                Score: 95,
                ProportionalDifference:
                    proportionalDifference,
                Available: true);
        }

        if (proportionalDifference <= 0.05)
        {
            return new DurationMatchResult(
                Score: 85,
                ProportionalDifference:
                    proportionalDifference,
                Available: true);
        }

        if (proportionalDifference <= 0.10)
        {
            return new DurationMatchResult(
                Score: 65,
                ProportionalDifference:
                    proportionalDifference,
                Available: true);
        }

        if (proportionalDifference <= 0.20)
        {
            return new DurationMatchResult(
                Score: 30,
                ProportionalDifference:
                    proportionalDifference,
                Available: true);
        }

        return new DurationMatchResult(
            Score: 0,
            ProportionalDifference:
                proportionalDifference,
            Available: true);
    }

    // ============================================================
    // BPM
    // ============================================================

    private static BPMMatchResult CalculateBPMScore(
        double? localBPM,
        double? candidateBPM)
    {
        if (!localBPM.HasValue ||
            !candidateBPM.HasValue ||
            localBPM.Value <= 0 ||
            candidateBPM.Value <= 0)
        {
            return new BPMMatchResult(
                0,
                false);
        }

        var local =
            localBPM.Value;

        var candidate =
            candidateBPM.Value;

        //
        // Direct BPM comparison.
        //

        var directDifference =
            Math.Abs(
                local -
                candidate);

        if (directDifference <= 1)
        {
            return new BPMMatchResult(
                100,
                false);
        }

        if (directDifference <= 2)
        {
            return new BPMMatchResult(
                90,
                false);
        }

        if (directDifference <= 4)
        {
            return new BPMMatchResult(
                70,
                false);
        }

        //
        // Half/double-time comparison.
        //

        var half =
            candidate / 2.0;

        var doubleTempo =
            candidate * 2.0;

        var halfDifference =
            Math.Abs(
                local -
                half);

        var doubleDifference =
            Math.Abs(
                local -
                doubleTempo);

        if (halfDifference <= 1 ||
            doubleDifference <= 1)
        {
            return new BPMMatchResult(
                100,
                true);
        }

        if (halfDifference <= 2 ||
            doubleDifference <= 2)
        {
            return new BPMMatchResult(
                90,
                true);
        }

        if (halfDifference <= 4 ||
            doubleDifference <= 4)
        {
            return new BPMMatchResult(
                70,
                true);
        }

        //
        // Small percentage differences can occur between
        // different analysis engines.
        //

        var percentageDifference =
            Math.Abs(
                local -
                candidate) /
            local *
            100;

        if (percentageDifference <= 3)
        {
            return new BPMMatchResult(
                60,
                false);
        }

        if (percentageDifference <= 6)
        {
            return new BPMMatchResult(
                40,
                false);
        }

        return new BPMMatchResult(
            0,
            false);
    }

    // ============================================================
    // Weighted Score
    // ============================================================

    private static void AddWeightedScore(
        ref double weightedScore,
        ref double availableWeight,
        double score,
        double weight,
        bool available)
    {
        if (!available)
            return;

        weightedScore +=
            score * weight;

        availableWeight +=
            weight;
    }

    // ============================================================
    // Reason
    // ============================================================

    private static string BuildReason(
        double artistScore,
        double titleScore,
        double durationScore,
        double durationDifference,
        bool durationAvailable,
        double bpmScore,
        bool bpmHalfDoubleMatch,
        TimeSpan? localDuration,
        TimeSpan? candidateDuration,
        double? localBPM,
        double? candidateBPM,
        bool severeDurationConflict,
        bool filenameHypothesisUsed,
        bool isMatch)
    {
        var reasons =
            new List<string>();

        if (filenameHypothesisUsed)
        {
            reasons.Add(
                "Matched against filename search hypothesis");
        }

        if (artistScore >= 95)
        {
            reasons.Add(
                "Artist matches strongly");
        }
        else if (artistScore >= 70)
        {
            reasons.Add(
                "Artist is a strong match");
        }
        else if (artistScore > 0)
        {
            reasons.Add(
                "Artist is a partial match");
        }
        else
        {
            reasons.Add(
                "Artist does not match");
        }

        if (titleScore >= 95)
        {
            reasons.Add(
                "Title matches strongly");
        }
        else if (titleScore >= 70)
        {
            reasons.Add(
                "Title is a strong match");
        }
        else if (titleScore > 0)
        {
            reasons.Add(
                "Title is a partial match");
        }
        else
        {
            reasons.Add(
                "Title does not match");
        }

        if (durationAvailable &&
            localDuration.HasValue &&
            candidateDuration.HasValue)
        {
            var seconds =
                Math.Abs(
                    (
                        localDuration.Value -
                        candidateDuration.Value)
                    .TotalSeconds);

            var percentage =
                durationDifference * 100;

            if (durationScore >= 95)
            {
                reasons.Add(
                    $"Duration matches within " +
                    $"{seconds:0.#} seconds");
            }
            else if (durationScore >= 65)
            {
                reasons.Add(
                    $"Duration differs by " +
                    $"{percentage:0.#}%");
            }
            else if (severeDurationConflict)
            {
                reasons.Add(
                    $"Duration differs significantly " +
                    $"({percentage:0.#}%) and indicates " +
                    $"a different recording/version");
            }
            else
            {
                reasons.Add(
                    $"Duration differs by " +
                    $"{seconds:0.#} seconds");
            }
        }

        if (localBPM.HasValue &&
            candidateBPM.HasValue)
        {
            if (bpmHalfDoubleMatch)
            {
                reasons.Add(
                    $"BPM has a half/double-time relationship " +
                    $"({localBPM.Value:0.###} / " +
                    $"{candidateBPM.Value:0.###})");
            }
            else if (bpmScore >= 90)
            {
                reasons.Add(
                    $"BPM matches closely " +
                    $"({candidateBPM.Value:0.###})");
            }
            else if (bpmScore > 0)
            {
                reasons.Add(
                    $"BPM is similar " +
                    $"({candidateBPM.Value:0.###})");
            }
            else
            {
                reasons.Add(
                    $"BPM differs " +
                    $"({localBPM.Value:0.###} vs " +
                    $"{candidateBPM.Value:0.###})");
            }
        }

        reasons.Add(
            isMatch
                ? "Candidate accepted for further analysis"
                : "Candidate rejected as a strong recording match");

        return string.Join(
            "; ",
            reasons);
    }

    // ============================================================
    // Artist Helpers
    // ============================================================

    private static List<string> SplitArtistNames(
        string value)
    {
        return value
            .Split(
                new[]
                {
                    ",",
                    ";",
                    " & ",
                    " feat. ",
                    " feat ",
                    " featuring ",
                    " ft. ",
                    " ft "
                },
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(
                NormaliseArtist)
            .Where(
                x => !string.IsNullOrWhiteSpace(x))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool NamesEquivalent(
        string left,
        string right)
    {
        if (left.Equals(
                right,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return left.Contains(
                   right,
                   StringComparison.OrdinalIgnoreCase) ||
               right.Contains(
                   left,
                   StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // String Similarity
    // ============================================================

    private static double CalculateTokenSimilarity(
        string left,
        string right)
    {
        var leftTokens =
            Tokenise(left);

        var rightTokens =
            Tokenise(right);

        if (leftTokens.Count == 0 ||
            rightTokens.Count == 0)
        {
            return 0;
        }

        var intersection =
            leftTokens
                .Intersect(
                    rightTokens,
                    StringComparer.OrdinalIgnoreCase)
                .Count();

        var union =
            leftTokens
                .Union(
                    rightTokens,
                    StringComparer.OrdinalIgnoreCase)
                .Count();

        if (union == 0)
            return 0;

        return
            (double)intersection /
            union *
            100;
    }

    private static List<string> Tokenise(
        string value)
    {
        return value
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(
                x => x.Length > 0)
            .ToList();
    }

    // ============================================================
    // Normalisation
    // ============================================================

    private static string NormaliseArtist(
        string? value)
    {
        return Normalise(value);
    }

    private static string NormaliseTitle(
        string? value)
    {
        return Normalise(value);
    }

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
                 value.Trim().ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            builder.Append(' ');
        }

        return string.Join(
            " ",
            builder
                .ToString()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries));
    }

    // ============================================================
    // Result Types
    // ============================================================

    private readonly record struct IdentityHypothesis(
        string Artist,
        string Title,
        bool UsedFilenameHypothesis);

    private readonly record struct BPMMatchResult(
        double Score,
        bool HalfDoubleMatch);

    private readonly record struct DurationMatchResult(
        double Score,
        double ProportionalDifference,
        bool Available);
}