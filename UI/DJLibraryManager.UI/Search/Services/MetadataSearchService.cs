using DJLibraryManager.UI.Analysis.Models;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Interfaces;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DJLibraryManager.UI.Search.Services;

/// <summary>
/// Performs metadata discovery for a single DIASISS library issue.
///
/// Search is provider-independent and evidence-driven.
///
/// Providers are searched independently. Their results are converted
/// into MetadataEvidence and independently analysed against the
/// original local track.
///
/// Recording identity and metadata consensus are deliberately
/// separated.
///
/// After the primary recording search has completed, unresolved
/// Year and Genre fields may trigger a second-stage metadata
/// enrichment search. Enrichment is driven by the established
/// primary recording identity and never replaces that identity.
///
/// This service does not modify the DIASISS library.
/// </summary>
public sealed class MetadataSearchService : ISearchService
{
    // ============================================================
    // Dependencies
    // ============================================================

    private readonly IReadOnlyList<IMetadataSearchProvider> _providers;

    private readonly MetadataEvidenceAnalysisService
        _evidenceAnalysisService;

    private readonly MetadataConsensusService
        _consensusService;

    private readonly MetadataRecommendationService
        _recommendationService;

    private readonly MetadataEnrichmentService
        _enrichmentService;

    // ============================================================
    // Configuration
    // ============================================================

    private const double MinimumArtistIdentityScore = 70.0;

    private const double MinimumTitleIdentityScore = 70.0;

    // ============================================================
    // Constructor
    // ============================================================

    public MetadataSearchService(
        IEnumerable<IMetadataSearchProvider> providers,
        MetadataEvidenceAnalysisService? evidenceAnalysisService = null,
        MetadataConsensusService? consensusService = null,
        MetadataRecommendationService? recommendationService = null,
        IEnumerable<IMetadataEnrichmentProvider>? enrichmentProviders = null)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers =
            providers
                .Where(
                    provider =>
                        provider is not null)
                .ToList();

        _evidenceAnalysisService =
            evidenceAnalysisService ??
            new MetadataEvidenceAnalysisService();

        _consensusService =
            consensusService ??
            new MetadataConsensusService();

        _recommendationService =
            recommendationService ??
            new MetadataRecommendationService();

        _enrichmentService =
            new MetadataEnrichmentService(
                enrichmentProviders ??
                Array.Empty<IMetadataEnrichmentProvider>());
    }

    // ============================================================
    // Search
    // ============================================================

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchIssue issue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(issue);

        cancellationToken.ThrowIfCancellationRequested();

        // ========================================================
        // Clear Previous Recommendations
        // ========================================================

        issue.MetadataRecommendations.Clear();

        if (string.IsNullOrWhiteSpace(issue.FilePath))
        {
            return [];
        }

        // ========================================================
        // Determine Missing Metadata
        // ========================================================

        var missingMetadata =
            GetMissingMetadata(
                issue);

        // ========================================================
        // Build Search Requests
        // ========================================================

        var requests =
            BuildSearchRequests(
                issue,
                missingMetadata);

        if (requests.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Original Local Track
        // ========================================================

        var localMedia =
            CreateLocalMediaItem(
                issue);

        // ========================================================
        // Provider Evidence
        // ========================================================

        var evidence =
            new List<MetadataEvidence>();

        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var provider in _providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<MetadataSearchProviderResult>
                    providerResults;

                try
                {
                    providerResults =
                        await provider.SearchAsync(
                            request,
                            cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    continue;
                }

                if (providerResults is null)
                {
                    continue;
                }

                foreach (var providerResult in providerResults)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (providerResult is null)
                    {
                        continue;
                    }

                    evidence.Add(
                        MetadataEvidenceFactory.Create(
                            providerResult));
                }
            }
        }

        if (evidence.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Candidate Analysis
        // ========================================================

        var analysedCandidates =
            _evidenceAnalysisService.Analyse(
                localMedia,
                evidence,
                issue.FilenameSearchHint);

        // ========================================================
        // Primary Field-Evidence Candidates
        // ========================================================
        //
        // Artist + Title are the primary recording identity.
        //
        // Duration, BPM and version differences must not prevent a
        // candidate from contributing useful metadata evidence.
        //

        var viableCandidates =
            analysedCandidates
                .Where(
                    IsEligibleForFieldConsensus)
                .OrderByDescending(
                    candidate =>
                        candidate.Match.Score)
                .ToList();

        if (viableCandidates.Count == 0)
        {
            return [];
        }

        // ========================================================
        // Primary Consensus
        // ========================================================

        var consensusResults =
            _consensusService.Analyse(
                viableCandidates);

        // ========================================================
        // Establish Recording Identity
        // ========================================================
        //
        // This is the critical boundary.
        //
        // Once the primary search has established Artist + Title,
        // enrichment must never be allowed to reopen that identity.
        //

        var establishedIdentity =
            DetermineEstablishedIdentity(
                issue,
                viableCandidates,
                consensusResults);

        // ========================================================
        // Restore Primary Identity Into Consensus
        // ========================================================
        //
        // Consensus can legitimately report a conflict for a field
        // even when the strongest primary recording identity is clear.
        //
        // Artist and Title are therefore explicitly locked here.
        //

        consensusResults =
            LockEstablishedIdentity(
                consensusResults,
                viableCandidates,
                establishedIdentity);

        // ========================================================
        // Additional Metadata Enrichment
        // ========================================================

        var enrichmentFields =
            GetUnresolvedEnrichmentFields(
                missingMetadata,
                consensusResults);

        if (enrichmentFields.Count > 0)
        {
            var enrichmentEvidence =
                await EnrichMissingMetadataAsync(
                    issue,
                    establishedIdentity,
                    enrichmentFields,
                    cancellationToken);

            if (enrichmentEvidence.Count > 0)
            {
                var enrichmentCandidates =
                    AnalyseEnrichmentEvidence(
                        issue,
                        establishedIdentity,
                        enrichmentEvidence);

                if (enrichmentCandidates.Count > 0)
                {
                    // ------------------------------------------------
                    // IMPORTANT:
                    //
                    // Do NOT add enrichment candidates to
                    // viableCandidates.
                    //
                    // They belong to a separate evidence stream.
                    // Adding them back into the primary collection
                    // allows enrichment providers to participate in
                    // Artist/Title consensus and can reopen the
                    // established recording identity.
                    // ------------------------------------------------

                    var enrichmentConsensusResults =
                        _consensusService.Analyse(
                            enrichmentCandidates);

                    // ------------------------------------------------
                    // Merge enrichment ONLY for the explicitly
                    // requested fields.
                    // ------------------------------------------------

                    consensusResults =
                        MergeEnrichmentConsensus(
                            consensusResults,
                            enrichmentConsensusResults,
                            enrichmentFields);

                    // ------------------------------------------------
                    // Re-lock Artist/Title/Album after enrichment.
                    //
                    // This makes the invariant explicit and protects
                    // against future changes to enrichment fields.
                    // ------------------------------------------------

                    consensusResults =
                        LockEstablishedIdentity(
                            consensusResults,
                            viableCandidates,
                            establishedIdentity);
                }
            }
        }

        // ========================================================
        // Metadata Recommendations
        // ========================================================

        var recommendations =
            _recommendationService.Recommend(
                consensusResults);

        AddRecommendationsToIssue(
            issue,
            recommendations);

        // ========================================================
        // Provider Evidence Results
        // ========================================================

        return CreateSearchResults(
            viableCandidates,
            issue);
    }

    // ============================================================
    // Field Consensus Eligibility
    // ============================================================

    private static bool IsEligibleForFieldConsensus(
        MetadataEvidenceAnalysisResult candidate)
    {
        if (candidate is null ||
            candidate.Match is null ||
            candidate.Evidence is null)
        {
            return false;
        }

        var artistStrong =
            candidate.Match.ArtistScore >=
            MinimumArtistIdentityScore;

        var titleStrong =
            candidate.Match.TitleScore >=
            MinimumTitleIdentityScore;

        return
            artistStrong &&
            titleStrong;
    }

    // ============================================================
    // Established Identity
    // ============================================================

    private static EstablishedMetadataIdentity
    DetermineEstablishedIdentity(
        SearchIssue issue,
        IReadOnlyList<
            MetadataEvidenceAnalysisResult>
            viableCandidates,
        IReadOnlyList<MetadataConsensusResult>
            consensusResults)
    {
        // --------------------------------------------------------
        // IMPORTANT:
        //
        // Artist and Title identity must be established from the
        // PRIMARY search consensus whenever a usable consensus
        // value exists.
        //
        // Do NOT allow the single strongest provider result to
        // override an established multi-provider identity.
        //
        // Example:
        //
        // Provider A: Luude
        // Provider B: Luude, Colin Hay
        //
        // If primary consensus establishes:
        //
        //     Luude, Colin Hay
        //
        // that becomes the established identity.
        //
        // Enrichment must then work from that identity.
        // --------------------------------------------------------

        var artist =
            GetConsensusValue(
                consensusResults,
                "Artist");

        var title =
            GetConsensusValue(
                consensusResults,
                "Title");

        var album =
            GetConsensusValue(
                consensusResults,
                "Album");

        // --------------------------------------------------------
        // If consensus does not provide a usable value, fall back
        // to the strongest PRIMARY candidate.
        //
        // This is a fallback only.
        // --------------------------------------------------------

        var strongest =
            viableCandidates
                .Where(
                    candidate =>
                        candidate.Match is not null &&
                        candidate.Evidence is not null)
                .OrderByDescending(
                    candidate =>
                        candidate.Match.Score)
                .FirstOrDefault();

        if (strongest is not null)
        {
            if (string.IsNullOrWhiteSpace(artist))
            {
                artist =
                    strongest.Evidence.Artist?.Trim()
                    ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title =
                    strongest.Evidence.Title?.Trim()
                    ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(album))
            {
                album =
                    strongest.Evidence.Album?.Trim()
                    ?? string.Empty;
            }
        }

        // --------------------------------------------------------
        // Finally fall back to the local issue.
        //
        // This is only a safety fallback.
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(artist))
        {
            artist =
                issue.Artist?.Trim()
                ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title =
                issue.TrackTitle?.Trim()
                ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(album))
        {
            album =
                issue.Album?.Trim()
                ?? string.Empty;
        }

        // --------------------------------------------------------
        // Preserve provider identities from the PRIMARY candidates.
        //
        // These identities are passed to the enrichment stage so
        // enrichment can query the exact recording already
        // identified by the primary search.
        // --------------------------------------------------------

        var providerIdentities =
            viableCandidates
                .Where(
                    candidate =>
                        candidate.Evidence is not null &&
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source) &&
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.ExternalId))
                .Select(
                    candidate =>
                        new MetadataProviderIdentity
                        {
                            Provider =
                                candidate.Evidence.Source,

                            ExternalId =
                                candidate.Evidence.ExternalId,

                            EntityType =
                                string.Empty
                        })
                .GroupBy(
                    identity =>
                        $"{identity.Provider}\u001F" +
                        $"{identity.ExternalId}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(
                    group =>
                        group.First())
                .ToList();

        return new EstablishedMetadataIdentity(
            artist,
            title,
            album,
            providerIdentities);
    }

    // ============================================================
    // Lock Established Identity
    // ============================================================

    private static IReadOnlyList<MetadataConsensusResult>
        LockEstablishedIdentity(
            IReadOnlyList<MetadataConsensusResult>
                consensusResults,
            IReadOnlyList<
                MetadataEvidenceAnalysisResult>
                viableCandidates,
            EstablishedMetadataIdentity identity)
    {
        var results =
            consensusResults.ToList();

        // --------------------------------------------------------
        // Artist
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(identity.Artist))
        {
            results =
                ReplaceIdentityConsensus(
                    results,
                    viableCandidates,
                    "Artist",
                    identity.Artist,
                    candidate =>
                        candidate.Evidence.Artist);
        }

        // --------------------------------------------------------
        // Title
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(identity.Title))
        {
            results =
                ReplaceIdentityConsensus(
                    results,
                    viableCandidates,
                    "Title",
                    identity.Title,
                    candidate =>
                        candidate.Evidence.Title);
        }

        // --------------------------------------------------------
        // Album
        //
        // Album is also part of the established identity, but only
        // lock it when the primary search actually established a
        // value.
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(identity.Album))
        {
            results =
                ReplaceIdentityConsensus(
                    results,
                    viableCandidates,
                    "Album",
                    identity.Album,
                    candidate =>
                        candidate.Evidence.Album);
        }

        return results;
    }

    private static List<MetadataConsensusResult>
        ReplaceIdentityConsensus(
            IReadOnlyList<MetadataConsensusResult>
                consensusResults,
            IReadOnlyList<
                MetadataEvidenceAnalysisResult>
                viableCandidates,
            string field,
            string selectedValue,
            Func<
                MetadataEvidenceAnalysisResult,
                string?>
                valueSelector)
    {
        var supportingProviders =
            viableCandidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source) &&
                        !string.IsNullOrWhiteSpace(
                            valueSelector(candidate)) &&
                        TextValuesEquivalent(
                            valueSelector(candidate)!,
                            selectedValue))
                .Select(
                    candidate =>
                        candidate.Evidence.Source)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var providersWithValue =
            viableCandidates
                .Where(
                    candidate =>
                        !string.IsNullOrWhiteSpace(
                            candidate.Evidence.Source) &&
                        !string.IsNullOrWhiteSpace(
                            valueSelector(candidate)))
                .Select(
                    candidate =>
                        candidate.Evidence.Source)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    provider =>
                        provider,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var conflictingProviders =
            providersWithValue
                .Except(
                    supportingProviders,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        // --------------------------------------------------------
        // If only one primary provider supplied the identity,
        // agreement is still 100% for the established identity.
        //
        // This is intentional: identity establishment and
        // cross-provider agreement are separate concepts.
        // --------------------------------------------------------

        var providerCount =
            supportingProviders.Count;

        if (providerCount == 0)
        {
            providerCount = 1;
        }

        var lockedResult =
            new MetadataConsensusResult
            {
                Field =
                    field,

                Value =
                    selectedValue,

                SupportingProviders =
                    supportingProviders.Count,

                ProvidersWithValue =
                    Math.Max(
                        supportingProviders.Count,
                        providersWithValue.Count),

                AgreementPercentage =
                    100,

                Strength =
                    MetadataConsensusStrength.Strong,

                SupportingSources =
                    supportingProviders,

                ConflictingSources =
                    conflictingProviders
            };

        var existingIndex =
            consensusResults
                .ToList()
                .FindIndex(
                    result =>
                        result.Field.Equals(
                            field,
                            StringComparison.OrdinalIgnoreCase));

        var results =
            consensusResults.ToList();

        if (existingIndex >= 0)
        {
            results[existingIndex] =
                lockedResult;
        }
        else
        {
            results.Add(
                lockedResult);
        }

        return results;
    }

    private static bool TextValuesEquivalent(
        string left,
        string right)
    {
        return
            string.Equals(
                NormaliseText(left),
                NormaliseText(right),
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormaliseText(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            " ",
            value
                .Trim()
                .ToUpperInvariant()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }

    // ============================================================
    // Consensus Value
    // ============================================================

    private static string GetConsensusValue(
        IReadOnlyList<MetadataConsensusResult>
            consensusResults,
        string field)
    {
        var result =
            consensusResults.FirstOrDefault(
                consensus =>
                    consensus.Field.Equals(
                        field,
                        StringComparison.OrdinalIgnoreCase));

        return
            result?.Value?.Trim()
            ?? string.Empty;
    }

    // ============================================================
    // Enrichment Fields
    // ============================================================

    private static IReadOnlyList<string>
        GetUnresolvedEnrichmentFields(
            IReadOnlyList<string>
                missingMetadata,
            IReadOnlyList<MetadataConsensusResult>
                consensusResults)
    {
        var fields =
            new List<string>();

        if (FieldRequiresEnrichment(
                "Year",
                missingMetadata,
                consensusResults))
        {
            fields.Add(
                "Year");
        }

        if (FieldRequiresEnrichment(
                "Genre",
                missingMetadata,
                consensusResults))
        {
            fields.Add(
                "Genre");
        }

        return fields;
    }

    private static bool FieldRequiresEnrichment(
        string field,
        IReadOnlyList<string>
            missingMetadata,
        IReadOnlyList<MetadataConsensusResult>
            consensusResults)
    {
        var wasMissing =
            missingMetadata.Any(
                value =>
                    value.Equals(
                        field,
                        StringComparison.OrdinalIgnoreCase));

        if (!wasMissing)
        {
            return false;
        }

        var consensus =
            consensusResults.FirstOrDefault(
                result =>
                    result.Field.Equals(
                        field,
                        StringComparison.OrdinalIgnoreCase));

        if (consensus is null)
        {
            return true;
        }

        return
            string.IsNullOrWhiteSpace(
                consensus.Value);
    }

    // ============================================================
    // Enrichment
    // ============================================================

    private async Task<IReadOnlyList<MetadataEvidence>>
        EnrichMissingMetadataAsync(
            SearchIssue issue,
            EstablishedMetadataIdentity identity,
            IReadOnlyList<string>
                enrichmentFields,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(identity.Artist) &&
            string.IsNullOrWhiteSpace(identity.Title))
        {
            return [];
        }

        var request =
            new MetadataEnrichmentRequest
            {
                Artist =
                    identity.Artist,

                Title =
                    identity.Title,

                Album =
                    identity.Album,

                ProviderIdentities =
                    identity.ProviderIdentities,

                MissingFields =
                    enrichmentFields,

                FilePath =
                    issue.FilePath,

                Duration =
                    issue.Duration
            };

        var providerResults =
            await _enrichmentService.EnrichAsync(
                request,
                cancellationToken);

        if (providerResults.Count == 0)
        {
            return [];
        }

        return
            providerResults
                .Where(
                    result =>
                        result is not null)
                .Select(
                    MetadataEvidenceFactory.Create)
                .ToList();
    }

    // ============================================================
    // Analyse Enrichment Evidence
    // ============================================================

    private IReadOnlyList<
        MetadataEvidenceAnalysisResult>
        AnalyseEnrichmentEvidence(
            SearchIssue issue,
            EstablishedMetadataIdentity identity,
            IReadOnlyList<MetadataEvidence>
                enrichmentEvidence)
    {
        var enrichmentMedia =
            CreateEnrichmentMediaItem(
                issue,
                identity);

        var analysed =
            _evidenceAnalysisService.Analyse(
                enrichmentMedia,
                enrichmentEvidence);

        return
            analysed
                .Where(
                    IsEligibleForFieldConsensus)
                .OrderByDescending(
                    candidate =>
                        candidate.Match.Score)
                .ToList();
    }

    // ============================================================
    // Enrichment Local Media
    // ============================================================

    private static DJLMMediaItem
        CreateEnrichmentMediaItem(
            SearchIssue issue,
            EstablishedMetadataIdentity identity)
    {
        return new DJLMMediaItem
        {
            FilePath =
                issue.FilePath,

            Artist =
                identity.Artist,

            Title =
                identity.Title,

            Album =
                identity.Album,

            Genre =
                issue.Genre ??
                string.Empty,

            Year =
                issue.Year,

            BPM =
                issue.Bpm,

            Key =
                issue.Key ??
                string.Empty,

            Duration =
                issue.Duration
        };
    }

    // ============================================================
    // Merge Enrichment Consensus
    // ============================================================

    private static IReadOnlyList<MetadataConsensusResult>
        MergeEnrichmentConsensus(
            IReadOnlyList<MetadataConsensusResult>
                primaryConsensusResults,
            IReadOnlyList<MetadataConsensusResult>
                enrichmentConsensusResults,
            IReadOnlyList<string>
                enrichmentFields)
    {
        var results =
            primaryConsensusResults.ToList();

        foreach (var field in enrichmentFields)
        {
            var enrichmentResult =
                enrichmentConsensusResults.FirstOrDefault(
                    result =>
                        result.Field.Equals(
                            field,
                            StringComparison.OrdinalIgnoreCase));

            if (enrichmentResult is null)
            {
                continue;
            }

            // ----------------------------------------------------
            // If enrichment has no usable value, preserve the
            // original primary consensus result.
            // ----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    enrichmentResult.Value))
            {
                continue;
            }

            var existingIndex =
                results.FindIndex(
                    result =>
                        result.Field.Equals(
                            field,
                            StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                results[existingIndex] =
                    enrichmentResult;
            }
            else
            {
                results.Add(
                    enrichmentResult);
            }
        }

        return results;
    }

    // ============================================================
    // Missing Metadata
    // ============================================================

    private static IReadOnlyList<string>
        GetMissingMetadata(
            SearchIssue issue)
    {
        if (issue.MissingFields is null ||
            issue.MissingFields.Count == 0)
        {
            return [];
        }

        return
            issue.MissingFields
                .Where(
                    field =>
                        !string.IsNullOrWhiteSpace(field))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    // ============================================================
    // Search Requests
    // ============================================================

    private static IReadOnlyList<MetadataSearchRequest>
        BuildSearchRequests(
            SearchIssue issue,
            IReadOnlyList<string>
                missingMetadata)
    {
        var requests =
            new List<MetadataSearchRequest>();

        var artist =
            issue.Artist?.Trim()
            ?? string.Empty;

        var title =
            issue.TrackTitle?.Trim()
            ?? string.Empty;

        // --------------------------------------------------------
        // Filename hypotheses
        // --------------------------------------------------------

        if (issue.FilenameSearchHint is not null)
        {
            foreach (var candidate in
                     issue.FilenameSearchHint.Candidates)
            {
                if (candidate is null)
                {
                    continue;
                }

                var candidateArtist =
                    candidate.Artist?.Trim()
                    ?? string.Empty;

                var candidateTitle =
                    candidate.Title?.Trim()
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(
                        candidateArtist) &&
                    string.IsNullOrWhiteSpace(
                        candidateTitle))
                {
                    continue;
                }

                requests.Add(
                    new MetadataSearchRequest
                    {
                        Artist =
                            !string.IsNullOrWhiteSpace(
                                artist)
                                ? artist
                                : candidateArtist,

                        Title =
                            !string.IsNullOrWhiteSpace(
                                title)
                                ? title
                                : candidateTitle
                    });
            }
        }

        // --------------------------------------------------------
        // Normal issue request
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(artist) ||
            !string.IsNullOrWhiteSpace(title))
        {
            requests.Add(
                new MetadataSearchRequest
                {
                    Artist =
                        artist,

                    Title =
                        title
                });
        }

        return
            requests
                .Where(
                    request =>
                        !string.IsNullOrWhiteSpace(
                            request.Artist) ||
                        !string.IsNullOrWhiteSpace(
                            request.Title))
                .GroupBy(
                    request =>
                        $"{request.Artist}\u001F" +
                        $"{request.Title}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(
                    group =>
                        group.First())
                .ToList();
    }

    // ============================================================
    // Local Media
    // ============================================================

    private static DJLMMediaItem
        CreateLocalMediaItem(
            SearchIssue issue)
    {
        return new DJLMMediaItem
        {
            FilePath =
                issue.FilePath,

            Artist =
                issue.Artist ??
                string.Empty,

            Title =
                issue.TrackTitle ??
                string.Empty,

            Album =
                issue.Album ??
                string.Empty,

            Genre =
                issue.Genre ??
                string.Empty,

            Year =
                issue.Year,

            BPM =
                issue.Bpm,

            Key =
                issue.Key ??
                string.Empty,

            Duration =
                issue.Duration
        };
    }

    // ============================================================
    // Recommendations
    // ============================================================

    private static void AddRecommendationsToIssue(
        SearchIssue issue,
        IReadOnlyList<MetadataChangeRecommendation>
            recommendations)
    {
        ArgumentNullException.ThrowIfNull(issue);

        ArgumentNullException.ThrowIfNull(
            recommendations);

        issue.MetadataRecommendations.Clear();

        foreach (var recommendation in recommendations)
        {
            if (recommendation is null)
            {
                continue;
            }

            issue.MetadataRecommendations.Add(
                new MetadataChangeRecommendation
                {
                    Field =
                        recommendation.Field,

                    CurrentValue =
                        GetCurrentValue(
                            issue,
                            recommendation.Field),

                    RecommendedValue =
                        recommendation.RecommendedValue,

                    AgreementPercentage =
                        recommendation.AgreementPercentage,

                    SupportingProviders =
                        recommendation.SupportingProviders,

                    ProvidersWithValue =
                        recommendation.ProvidersWithValue,

                    Strength =
                        recommendation.Strength,

                    IsRecommended =
                        recommendation.IsRecommended,

                    IsSelected =
                        false,

                    Reason =
                        recommendation.Reason
                });
        }
    }

    // ============================================================
    // Current Value
    // ============================================================

    private static string GetCurrentValue(
        SearchIssue issue,
        string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return string.Empty;
        }

        if (field.Equals(
                "Artist",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Artist ??
                   string.Empty;
        }

        if (field.Equals(
                "Title",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.TrackTitle ??
                   string.Empty;
        }

        if (field.Equals(
                "Album",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Album ??
                   string.Empty;
        }

        if (field.Equals(
                "Genre",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Genre ??
                   string.Empty;
        }

        if (field.Equals(
                "Year",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Year?.ToString()
                ?? string.Empty;
        }

        if (field.Equals(
                "BPM",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Bpm?.ToString(
                       "0.###")
                   ?? string.Empty;
        }

        if (field.Equals(
                "Key",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Key ??
                   string.Empty;
        }

        if (field.Equals(
                "Duration",
                StringComparison.OrdinalIgnoreCase))
        {
            return issue.Duration.HasValue
                ? issue.Duration.Value.ToString(
                    @"mm\:ss")
                : string.Empty;
        }

        return string.Empty;
    }

    // ============================================================
    // Search Results
    // ============================================================

    private static IReadOnlyList<SearchResult>
        CreateSearchResults(
            IReadOnlyList<
                MetadataEvidenceAnalysisResult>
                viableCandidates,
            SearchIssue issue)
    {
        var results =
            new List<SearchResult>();

        foreach (var candidate in viableCandidates)
        {
            var evidence =
                candidate.Evidence;

            results.Add(
                new SearchResult
                {
                    Id =
                        Guid.NewGuid().ToString(),

                    MediaId =
                        issue.MediaId,

                    Source =
                        evidence.Source,

                    MatchScore =
                        Math.Clamp(
                            candidate.Match.Score,
                            0,
                            100),

                    IsRecommended =
                        false,

                    RecommendationReason =
                        candidate.Match.Reason,

                    Artist =
                        evidence.Artist,

                    TrackTitle =
                        evidence.Title,

                    Album =
                        evidence.Album,

                    Genre =
                        evidence.Genre,

                    Bpm =
                        evidence.BPM,

                    Key =
                        evidence.Key,

                    Duration =
                        evidence.Duration,

                    FilePath =
                        issue.FilePath,

                    FileExists =
                        File.Exists(
                            issue.FilePath),

                    IntegrityStatus =
                        string.Empty
                });
        }

        return results;
    }

    // ============================================================
    // Established Identity Model
    // ============================================================

    private sealed record EstablishedMetadataIdentity(
        string Artist,
        string Title,
        string Album,
        IReadOnlyList<MetadataProviderIdentity>
            ProviderIdentities);
}