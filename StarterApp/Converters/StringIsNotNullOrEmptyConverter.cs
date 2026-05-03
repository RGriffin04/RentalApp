using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converter that checks if a string is not null or empty
/// </summary>
public class StringIsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
