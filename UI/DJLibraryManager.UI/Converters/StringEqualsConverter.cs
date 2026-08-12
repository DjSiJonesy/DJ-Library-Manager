using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DJLibraryManager.UI.Converters;

/// <summary>
/// Compares the bound value with the supplied converter parameter.
/// Returns true when they are equal.
/// </summary>
public sealed class StringEqualsConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is null || parameter is null)
            return false;

        return string.Equals(
            value.ToString(),
            parameter.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}