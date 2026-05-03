using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converts a boolean availability status to a readable string
/// </summary>
public class AvailabilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "Available" : "Not Available";
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
