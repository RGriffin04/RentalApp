using System.Globalization;
using RentalApp.Helpers;

namespace RentalApp.Converters;

/// <summary>
/// Converts DateTime to UK short date format string (dd MMM yyyy)
/// Used in XAML bindings to display compact dates
/// </summary>
public class ShortDateToUkStringConverter : IValueConverter
{
    /// <summary>
    /// Convert DateTime to UK short format string
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return CultureHelper.FormatShortDate(dateTime);
        }
        return string.Empty;
    }

    /// <summary>
    /// Convert back is not supported for display-only converter
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Short date converter does not support ConvertBack");
    }
}
