using System.Globalization;
using RentalApp.Helpers;

namespace RentalApp.Converters;

/// <summary>
/// Converts decimal values to UK currency format (£)
/// Used in XAML bindings to display prices in GBP
/// </summary>
public class CurrencyToUkStringConverter : IValueConverter
{
    /// <summary>
    /// Convert decimal amount to GBP format string
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return CultureHelper.FormatCurrency(amount);
        }
        if (value is double doubleAmount)
        {
            return CultureHelper.FormatCurrency((decimal)doubleAmount);
        }
        if (value is int intAmount)
        {
            return CultureHelper.FormatCurrency((decimal)intAmount);
        }
        return "£0.00";
    }

    /// <summary>
    /// Convert back is not supported for currency display
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Currency converter does not support ConvertBack");
    }
}
