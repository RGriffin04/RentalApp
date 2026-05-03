using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converter that checks if a string equals a parameter value
/// </summary>
public class StringEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && parameter is string paramValue)
        {
            return stringValue.Equals(paramValue, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
