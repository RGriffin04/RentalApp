using System.Globalization;
using RentalApp.Helpers;

namespace RentalApp.Converters;

/// <summary>
/// Converts DateTime to UK date format string (dd/MM/yyyy)
/// Used in XAML bindings to display dates in UK format
/// </summary>
public class DateToUkStringConverter : IValueConverter
{
    /// <summary>
    /// Convert DateTime to UK format string
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return CultureHelper.FormatDate(dateTime);
        }
        return string.Empty;
    }

    /// <summary>
    /// Convert UK format string back to DateTime
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string dateString)
        {
            return CultureHelper.ParseDate(dateString);
        }
        return null;
    }
}
