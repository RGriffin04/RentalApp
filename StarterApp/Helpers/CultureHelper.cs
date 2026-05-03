using System.Globalization;

namespace RentalApp.Helpers;

/// <summary>
/// Helper class for UK date and number formatting
/// Provides consistent formatting across the application using en-GB culture
/// </summary>
public static class CultureHelper
{
    /// <summary>
    /// UK culture info (en-GB)
    /// </summary>
    public static CultureInfo UkCulture { get; } = new CultureInfo("en-GB");

    /// <summary>
    /// Format date in UK format (dd/MM/yyyy)
    /// </summary>
    /// <param name="date">Date to format</param>
    /// <returns>Formatted date string (e.g., "25/12/2024")</returns>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("dd/MM/yyyy", UkCulture);
    }

    /// <summary>
    /// Format date and time in UK format (dd/MM/yyyy HH:mm)
    /// </summary>
    /// <param name="dateTime">DateTime to format</param>
    /// <returns>Formatted datetime string (e.g., "25/12/2024 14:30")</returns>
    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy HH:mm", UkCulture);
    }

    /// <summary>
    /// Format short date in UK format (dd MMM yyyy)
    /// </summary>
    /// <param name="date">Date to format</param>
    /// <returns>Formatted date string (e.g., "25 Dec 2024")</returns>
    public static string FormatShortDate(DateTime date)
    {
        return date.ToString("dd MMM yyyy", UkCulture);
    }

    /// <summary>
    /// Format currency in GBP (£)
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <returns>Formatted currency string (e.g., "£123.45")</returns>
    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", UkCulture); // £123.45
    }

    /// <summary>
    /// Format currency per day
    /// </summary>
    /// <param name="amount">Daily rate amount</param>
    /// <returns>Formatted currency string with "per day" (e.g., "£25.00 per day")</returns>
    public static string FormatDailyRate(decimal amount)
    {
        return $"{FormatCurrency(amount)} per day";
    }

    /// <summary>
    /// Format distance in km with UK formatting
    /// </summary>
    /// <param name="distanceKm">Distance in kilometers</param>
    /// <returns>Formatted distance string (e.g., "2.5 km")</returns>
    public static string FormatDistance(double distanceKm)
    {
        return $"{distanceKm:N1} km"; // 2.5 km
    }

    /// <summary>
    /// Parse UK date string to DateTime
    /// </summary>
    /// <param name="dateString">Date string in dd/MM/yyyy format</param>
    /// <returns>Parsed DateTime or null if invalid</returns>
    public static DateTime? ParseDate(string dateString)
    {
        if (DateTime.TryParseExact(
            dateString, 
            "dd/MM/yyyy", 
            UkCulture, 
            DateTimeStyles.None, 
            out var result))
        {
            return result;
        }
        return null;
    }

    /// <summary>
    /// Calculate days between two dates
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Number of days</returns>
    public static int CalculateDays(DateTime startDate, DateTime endDate)
    {
        return (endDate.Date - startDate.Date).Days;
    }

    /// <summary>
    /// Format date range in UK format
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Formatted date range (e.g., "25/12/2024 - 31/12/2024")</returns>
    public static string FormatDateRange(DateTime startDate, DateTime endDate)
    {
        return $"{FormatDate(startDate)} - {FormatDate(endDate)}";
    }
}
