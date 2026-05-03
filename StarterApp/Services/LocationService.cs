namespace RentalApp.Services;

/// <summary>
/// Service implementation for device location operations
/// Uses MAUI Geolocation and Geocoding APIs
/// </summary>
public class LocationService : ILocationService
{
    /// <summary>
    /// Gets the device's current GPS location
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            // Check permission first
            var hasPermission = await CheckLocationPermissionAsync();
            if (!hasPermission)
            {
                hasPermission = await RequestLocationPermissionAsync();
                if (!hasPermission)
                    return null;
            }

            // Request location with best accuracy, 10 second timeout
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                return (location.Latitude, location.Longitude);
            }

            return null;
        }
        catch (Exception)
        {
            // Location services disabled or other error
            return null;
        }
    }

    /// <summary>
    /// Requests location permission from the user
    /// </summary>
    public async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if location permission has been granted
    /// </summary>
    public async Task<bool> CheckLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a human-readable address from GPS coordinates (reverse geocoding)
    /// </summary>
    public async Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var location = new Location(latitude, longitude);
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                // Format address in UK style
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(placemark.Thoroughfare))
                    parts.Add(placemark.Thoroughfare); // Street name

                if (!string.IsNullOrWhiteSpace(placemark.Locality))
                    parts.Add(placemark.Locality); // City/Town

                if (!string.IsNullOrWhiteSpace(placemark.AdminArea))
                    parts.Add(placemark.AdminArea); // County/Region

                if (!string.IsNullOrWhiteSpace(placemark.PostalCode))
                    parts.Add(placemark.PostalCode); // Postcode

                return string.Join(", ", parts);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets GPS coordinates from an address (geocoding)
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GetCoordinatesFromAddressAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var locations = await Geocoding.Default.GetLocationsAsync(address);
            var location = locations?.FirstOrDefault();

            if (location != null)
            {
                return (location.Latitude, location.Longitude);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
