using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Converts a boolean value to custom text strings
/// ConverterParameter format: "TrueText|FalseText"
/// Example: ConverterParameter="Update Item|Create Item"
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return parameter?.ToString()?.Split('|').LastOrDefault() ?? "False";

        var texts = parameter?.ToString()?.Split('|');

        if (texts == null || texts.Length < 2)
            return boolValue.ToString();

        return boolValue ? texts[0] : texts[1];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
