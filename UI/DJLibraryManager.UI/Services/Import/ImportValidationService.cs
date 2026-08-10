using System;

using DJLibraryManager.Core.Models;

namespace DJLibraryManager.UI.Services.Import;

/// <summary>
/// Validates whether a media location import is still current with the
/// latest Discovery results.
/// </summary>
public sealed class ImportValidationService
{
    /// <summary>
    /// Returns true if the import is no longer current.
    /// </summary>
    public bool HasChanges(
        DiscoverySession discoverySession,
        Models.Import.MediaImportRecord importRecord)
    {
        ArgumentNullException.ThrowIfNull(discoverySession);
        ArgumentNullException.ThrowIfNull(importRecord);

        //
        // If the discovery was performed after the import then the
        // media location must be re-imported.
        //
        if (importRecord.DiscoveryDate != discoverySession.DiscoveryDate)
            return true;

        //
        // Folder count changed.
        //
        if (importRecord.FolderCount != discoverySession.FolderCount)
            return true;

        //
        // Audio file count changed.
        //
        if (importRecord.AudioFileCount != discoverySession.AudioFileCount)
            return true;

        //
        // Video file count changed.
        //
        if (importRecord.VideoFileCount != discoverySession.VideoFileCount)
            return true;

        return false;
    }
}