using DJLibraryManager.Core.Models;

using System;
using System.Linq;

namespace DJLibraryManager.UI.Services.Discovery;

/// <summary>
/// Performs a lightweight validation of a previously discovered media location.
/// This reuses the existing MediaLibraryDiscoveryService so that validation
/// and discovery always use identical counting rules.
/// </summary>
public sealed class DiscoveryValidationService
{
    private readonly MediaLibraryDiscoveryService _discoveryService = new();

    /// <summary>
    /// Returns true if the media location has changed since the last discovery.
    /// </summary>
    public bool HasChanges(DiscoverySession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        try
        {
            //
            // Re-run discovery using the SAME logic used by the
            // Discovery workflow.
            //

            var libraries =
                _discoveryService.DiscoverLibraries(
                    session.MediaLocation);

            var currentFolderCount = libraries.Count;

            var currentAudioCount =
                libraries.Sum(x => x.AudioFileCount);

            var currentVideoCount =
                libraries.Sum(x => x.VideoFileCount);

#if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"[DiscoveryValidation] Stored : " +
                $"Folders={session.FolderCount}, " +
                $"Audio={session.AudioFileCount}, " +
                $"Video={session.VideoFileCount}");

            System.Diagnostics.Debug.WriteLine(
                $"[DiscoveryValidation] Current: " +
                $"Folders={currentFolderCount}, " +
                $"Audio={currentAudioCount}, " +
                $"Video={currentVideoCount}");
#endif

            return
                currentFolderCount != session.FolderCount ||
                currentAudioCount != session.AudioFileCount ||
                currentVideoCount != session.VideoFileCount;
        }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"[DiscoveryValidation] Validation failed: {ex}");
#endif

            // If validation cannot be completed, assume changes.
            return true;
        }
    }
}