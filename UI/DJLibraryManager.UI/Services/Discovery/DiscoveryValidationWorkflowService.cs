using DJLibraryManager.Core.Models;

using DJLibraryManager.UI.Models.Discovery;

using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Services.Discovery;

/// <summary>
/// Coordinates validation of one or more discovered media locations.
/// </summary>
public sealed class DiscoveryValidationWorkflowService
{
    private readonly DiscoveryValidationService _validationService = new();

    private readonly DiscoveryValidationRepository _repository;

    public DiscoveryValidationWorkflowService(
        DiscoveryValidationRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Validates every Discovery Session and replaces the cached
    /// validation results.
    /// </summary>
    public void Validate(
        IEnumerable<DiscoverySession> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);

        var records = new List<DiscoveryValidationRecord>();

        foreach (var session in sessions)
        {
            records.Add(new DiscoveryValidationRecord
            {
                LocationPath = session.MediaLocation.Path,
                LastValidated = DateTime.Now,
                HasChanges = _validationService.HasChanges(session)
            });
        }

        _repository.SaveAll(records);
    }

    /// <summary>
    /// Validates a single Discovery Session and updates only its
    /// cached validation record.
    /// </summary>
    public void Validate(
        DiscoverySession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _repository.Save(
            new DiscoveryValidationRecord
            {
                LocationPath = session.MediaLocation.Path,
                LastValidated = DateTime.Now,
                HasChanges = _validationService.HasChanges(session)
            });
    }
}