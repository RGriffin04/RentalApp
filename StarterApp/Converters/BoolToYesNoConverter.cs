using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converter that converts a boolean to "Available" or "Unavailable" text
/// </summary>
public class BoolToYesNoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Available" : "Unavailable";
        }
        return "Unknown";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
