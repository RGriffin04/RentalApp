using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converter that converts an integer to boolean (useful for tab selection)
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
        {
            return intValue == paramValue;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
