namespace RentalApp.Services;

/// <summary>
/// Service interface for device location operations
/// Wraps MAUI Geolocation API for testability
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the device's current GPS location
    /// </summary>
    /// <returns>Tuple of (Latitude, Longitude) or null if unavailable</returns>
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();

    /// <summary>
    /// Requests location permission from the user
    /// </summary>
    /// <returns>True if permission granted</returns>
    Task<bool> RequestLocationPermissionAsync();

    /// <summary>
    /// Checks if location permission has been granted
    /// </summary>
    /// <returns>True if permission granted</returns>
    Task<bool> CheckLocationPermissionAsync();

    /// <summary>
    /// Gets a human-readable address from GPS coordinates (reverse geocoding)
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <returns>Address string or null if unavailable</returns>
    Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude);

    /// <summary>
    /// Gets GPS coordinates from an address (geocoding)
    /// </summary>
    /// <param name="address">Address string</param>
    /// <returns>Tuple of (Latitude, Longitude) or null if not found</returns>
    Task<(double Latitude, double Longitude)?> GetCoordinatesFromAddressAsync(string address);
}
