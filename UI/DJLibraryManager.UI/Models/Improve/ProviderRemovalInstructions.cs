using System.Collections.Generic;

namespace DJLibraryManager.UI.Models.Improve;

/// <summary>
/// Provides provider-specific instructions for removing missing-file
/// records from the originating DJ application's database.
/// 
/// DIASISS does not modify the provider database directly.
/// </summary>
public sealed class ProviderRemovalInstructions
{
    public string ProviderName { get; }

    public string Title { get; }

    public string Description { get; }

    public IReadOnlyList<string> Steps { get; }

    public ProviderRemovalInstructions(
        string providerName,
        string title,
        string description,
        IReadOnlyList<string> steps)
    {
        ProviderName = providerName;
        Title = title;
        Description = description;
        Steps = steps;
    }
}