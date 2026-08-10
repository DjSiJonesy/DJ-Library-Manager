using DJLibraryManager.Core.Models;

using DJLibraryManager.UI.Models.Discovery;

using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Services.Discovery;

/// <summary>
/// Coordinates validation of every discovered media location.
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
    /// Validates every Discovery Session and updates the cache.
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
}