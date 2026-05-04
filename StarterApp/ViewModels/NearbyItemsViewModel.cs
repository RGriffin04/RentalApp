/// @file NearbyItemsViewModel.cs
/// @brief View model for nearby items search
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// @brief View model for location-based item search
/// @details Uses device location or manual input to find nearby items
/// @extends BaseViewModel
public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly ILocationService _locationService;
    private readonly INavigationService _navigationService;

    /// @brief Collection of nearby items with distances
    [ObservableProperty]
    private ObservableCollection<ItemWithDistance> _items = new();

    /// @brief Current latitude
    [ObservableProperty]
    private double _latitude;

    /// @brief Current longitude
    [ObservableProperty]
    private double _longitude;

    /// @brief Search radius in kilometers
    [ObservableProperty]
    private double _radiusKm = 10.0;

    /// @brief Current address (from coordinates)
    [ObservableProperty]
    private string _currentAddress = string.Empty;

    /// @brief Whether location permission is granted
    [ObservableProperty]
    private bool _hasLocationPermission;

    /// @brief Default constructor for design-time support
    public NearbyItemsViewModel()
    {
        Title = "Nearby Items";
    }

    /// @brief Initializes a new instance of the NearbyItemsViewModel class
    public NearbyItemsViewModel(
        IItemService itemService,
        ILocationService locationService,
        INavigationService navigationService)
    {
        _itemService = itemService;
        _locationService = locationService;
        _navigationService = navigationService;
        Title = "Nearby Items";
    }

    /// @brief Gets current device location
    [RelayCommand]
    private async Task GetCurrentLocationAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Check permission
            HasLocationPermission = await _locationService.CheckLocationPermissionAsync();

            if (!HasLocationPermission)
            {
                HasLocationPermission = await _locationService.RequestLocationPermissionAsync();
            }

            if (!HasLocationPermission)
            {
                SetError("Location permission is required to find nearby items.");
                return;
            }

            // Get location
            var location = await _locationService.GetCurrentLocationAsync();

            if (location == null)
            {
                SetError("Unable to get current location. Please check your device settings.");
                return;
            }

            Latitude = location.Value.Latitude;
            Longitude = location.Value.Longitude;

            // Get address
            var address = await _locationService.GetAddressFromCoordinatesAsync(Latitude, Longitude);
            CurrentAddress = address ?? $"{Latitude:F4}, {Longitude:F4}";

            // Search nearby items
            await SearchNearbyAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to get location: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Searches for nearby items
    [RelayCommand]
    private async Task SearchNearbyAsync()
    {
        if (IsBusy)
            return;

        if (Latitude == 0 && Longitude == 0)
        {
            SetError("Please get your current location or enter coordinates.");
            return;
        }

        if (RadiusKm <= 0 || RadiusKm > 100)
        {
            SetError("Radius must be between 1 and 100 km.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var nearbyItems = await _itemService.GetNearbyItemsAsync(Latitude, Longitude, RadiusKm);

            Items.Clear();
            foreach (var item in nearbyItems)
            {
                Items.Add(item);
            }

            if (Items.Count == 0)
            {
                SetError($"No items found within {RadiusKm} km of your location.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to search nearby items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Navigates to item detail page
    /// @param item The item to view
    [RelayCommand]
    private async Task ViewItemAsync(Item item)
    {
        if (item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }
}
