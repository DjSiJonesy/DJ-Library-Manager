using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using DJLibraryManager.UI.Models.Search;

namespace DJLibraryManager.UI.Converters;

/// <summary>
/// Determines whether a SearchResult is the currently selected
/// result for a SearchIssue.
///
/// The converter compares:
///     SearchIssue.SelectedResultId
///     SearchResult.Id
///
/// The selected state is derived from the SearchIssue and is
/// therefore not persisted separately on SearchResult.
/// </summary>
public sealed class SearchResultSelectedConverter
    : IMultiValueConverter
{
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        if (values[0] is not SearchIssue issue)
            return false;

        if (values[1] is not SearchResult result)
            return false;

        return string.Equals(
            issue.SelectedResultId,
            result.Id,
            StringComparison.OrdinalIgnoreCase);
    }

    public object[] ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}