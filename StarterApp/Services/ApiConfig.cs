namespace RentalApp.Services;

/// <summary>
/// Configuration for API endpoints
/// </summary>
public static class ApiConfig
{
    /// <summary>
    /// Base URL for the API
    /// For Android emulator: Use 10.0.2.2 to access host machine's localhost
    /// For iOS simulator: Use localhost
    /// For physical devices: Use actual IP address or domain
    /// </summary>
#if ANDROID
    public static readonly string BaseUrl = "http://10.0.2.2:5000";
#elif IOS
    public static readonly string BaseUrl = "http://localhost:5000";
#else
    public static readonly string BaseUrl = "http://localhost:5000";
#endif

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the full URL for an API endpoint
    /// </summary>
    /// <param name="endpoint">Endpoint path (e.g., "/api/items")</param>
    /// <returns>Full URL</returns>
    public static string GetUrl(string endpoint)
    {
        return $"{BaseUrl}{endpoint}";
    }
}
