using DJLibraryManager.UI.Models.Improve;

using System;
using System.Collections.Generic;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Provides instructions for removing missing-file records from
/// supported DJ provider databases.
///
/// DIASISS does not modify provider databases directly.
/// </summary>
public sealed class ProviderRemovalInstructionsService
{
    /// <summary>
    /// Returns provider-specific instructions for the supplied provider.
    ///
    /// Returns null when the provider is not currently supported.
    /// </summary>
    public ProviderRemovalInstructions? GetInstructions(
        string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return null;

        if (string.Equals(
                providerName,
                "VirtualDJ",
                StringComparison.OrdinalIgnoreCase))
        {
            return new ProviderRemovalInstructions(
                "VirtualDJ",
                "Remove Missing Files from VirtualDJ",
                "VirtualDJ provides its own database cleanup function. " +
                "Use it to remove records for files that no longer exist.",
                new List<string>
                {
                    "Open VirtualDJ.",
                    "Open the Browser.",
                    "Open Browser Options.",
                    "Select Database.",
                    "Select Remove missing files from Search DB.",
                    "Return to DIASISS and Re-Import Virtual DJ Library again.",
                    "Re-Run the Analysis within DIASISS."
                });
        }

        return null;
    }
}