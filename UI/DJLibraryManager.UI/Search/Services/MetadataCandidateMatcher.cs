using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
/// The matcher identifies plausible candidate recordings only.
/// It does not resolve field-level metadata consensus and does not
/// make the final metadata recommendation.
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

    // ============================================================
    // Public API
    // ============================================================

    public MetadataCandidateMatch Match(
        DJLMMediaItem media,
        MetadataEvidence evidence)
    {
        return Match(
            media,
            evidence,
            filenameSearchHint: null);
    }

    public MetadataCandidateMatch Match(
        DJLMMediaItem media,
        MetadataEvidence evidence,
        FilenameSearchHint? filenameSearchHint)
    {
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(evidence);

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
                (
                    string.IsNullOrWhiteSpace(localArtist) &&
                    !string.IsNullOrWhiteSpace(candidate.Artist)
                )
                ||
                (
                    string.IsNullOrWhiteSpace(localTitle) &&
                    !string.IsNullOrWhiteSpace(candidate.Title)
                );

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
            durationResult.Available);

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

        // --------------------------------------------------------
        // Primary identity
        // --------------------------------------------------------
        //
        // Artist and Title establish the identity of the recording.
        //
        // Duration and BPM are supporting evidence.
        //
        // They must not veto an otherwise strong identity match,
        // because provider metadata may legitimately describe:
        //
        // - radio edits
        // - extended versions
        // - intro/outro differences
        // - video versions
        // - provider timing differences
        // - BPM analysis differences
        //
        // The final metadata decision is made field-by-field by
        // MetadataConsensusService.
        //

        var primaryIdentityMatch =
            artistScore >= 70 &&
            titleScore >= 70;

        var isMatch =
            primaryIdentityMatch &&
            overallScore >= MatchThreshold;

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

        var localCoreTitle =
            RemoveFeaturedArtistSuffix(
                local);

        var candidateCoreTitle =
            RemoveFeaturedArtistSuffix(
                candidate);

        if (!localCoreTitle.Equals(
                local,
                StringComparison.OrdinalIgnoreCase) ||
            !candidateCoreTitle.Equals(
                candidate,
                StringComparison.OrdinalIgnoreCase))
        {
            if (localCoreTitle.Equals(
                    candidateCoreTitle,
                    StringComparison.OrdinalIgnoreCase))
            {
                return 98;
            }

            if (candidateCoreTitle.Equals(
                    local,
                    StringComparison.OrdinalIgnoreCase) ||
                localCoreTitle.Equals(
                    candidate,
                    StringComparison.OrdinalIgnoreCase))
            {
                return 98;
            }

            if (candidateCoreTitle.Contains(
                    localCoreTitle,
                    StringComparison.OrdinalIgnoreCase) ||
                localCoreTitle.Contains(
                    candidateCoreTitle,
                    StringComparison.OrdinalIgnoreCase))
            {
                return 92;
            }
        }

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
    // Featured Artist
    // ============================================================

    private static string RemoveFeaturedArtistSuffix(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var patterns =
            new[]
            {
                @"\s*\(\s*FEAT(?:URING)?\.?\s+.+?\s*\)\s*$",
                @"\s*\[\s*FEAT(?:URING)?\.?\s+.+?\s*\]\s*$",
                @"\s*-\s*FEAT(?:URING)?\.?\s+.+?\s*$",
                @"\s+\bFEAT(?:URING)?\.?\s+.+?\s*$",
                @"\s+\bFT\.?\s+.+?\s*$"
            };

        foreach (var pattern in patterns)
        {
            var result =
                Regex.Replace(
                    value,
                    pattern,
                    string.Empty,
                    RegexOptions.IgnoreCase);

            if (!result.Equals(
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return result.Trim();
            }
        }

        return value.Trim();
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
                Available: false);
        }

        var localSeconds =
            localDuration.Value.TotalSeconds;

        var candidateSeconds =
            candidateDuration.Value.TotalSeconds;

        var absoluteDifference =
            Math.Abs(
                localSeconds -
                candidateSeconds);

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
        {
            return;
        }

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
            else
            {
                reasons.Add(
                    $"Duration differs significantly " +
                    $"({percentage:0.#}%)");
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
        {
            return 0;
        }

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