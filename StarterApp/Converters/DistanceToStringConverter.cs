using System.Globalization;
using RentalApp.Helpers;

namespace RentalApp.Converters;

/// <summary>
/// Converts distance values to formatted string with km unit
/// Used in XAML bindings to display distances in metric format
/// </summary>
public class DistanceToStringConverter : IValueConverter
{
    /// <summary>
    /// Convert distance to formatted string (e.g., "2.5 km")
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double distance)
        {
            return CultureHelper.FormatDistance(distance);
        }
        if (value is decimal decimalDistance)
        {
            return CultureHelper.FormatDistance((double)decimalDistance);
        }
        if (value is float floatDistance)
        {
            return CultureHelper.FormatDistance(floatDistance);
        }
        return "0.0 km";
    }

    /// <summary>
    /// Convert back is not supported for distance display
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Distance converter does not support ConvertBack");
    }
}
