/// @file CreateItemViewModel.cs
/// @brief View model for creating/editing items
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// @brief View model for creating or editing marketplace items
/// @extends BaseViewModel
[QueryProperty(nameof(ItemId), "itemId")]
public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly ILocationService _locationService;
    private readonly INavigationService _navigationService;

    /// @brief Item ID for editing (0 for new item)
    [ObservableProperty]
    private int _itemId;

    /// @brief Item title
    [ObservableProperty]
    private string _title = string.Empty;

    /// @brief Item description
    [ObservableProperty]
    private string _description = string.Empty;

    /// @brief Daily rental price
    [ObservableProperty]
    private decimal _dailyPrice;

    /// @brief Whether the item is available
    [ObservableProperty]
    private bool _isAvailable = true;

    /// @brief Item location address
    [ObservableProperty]
    private string _address = string.Empty;

    /// @brief Latitude
    [ObservableProperty]
    private double? _latitude;

    /// @brief Longitude
    [ObservableProperty]
    private double? _longitude;

    /// @brief Collection of categories
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    /// @brief Selected category
    [ObservableProperty]
    private Category? _selectedCategory;

    /// @brief Whether this is edit mode
    [ObservableProperty]
    private bool _isEditMode;

    /// @brief Default constructor for design-time support
    public CreateItemViewModel()
    {
        Title = "Create Item";
    }

    /// @brief Initializes a new instance of the CreateItemViewModel class
    public CreateItemViewModel(
        IItemService itemService,
        ILocationService locationService,
        INavigationService navigationService)
    {
        _itemService = itemService;
        _locationService = locationService;
        _navigationService = navigationService;
        Title = "Create Item";
    }

    /// @brief Loads categories and item data if editing
    [RelayCommand]
    private async Task InitializeAsync()
    {
        IsEditMode = ItemId > 0;
        Title = IsEditMode ? "Edit Item" : "Create Item";

        await LoadDataAsync();
    }

    /// @brief Loads categories and item (if editing)
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load categories
            var categoriesList = await _itemService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categoriesList)
            {
                Categories.Add(category);
            }

            // Load item if editing
            if (IsEditMode && ItemId > 0)
            {
                var item = await _itemService.GetItemByIdAsync(ItemId);
                if (item != null)
                {
                    Title = item.Title;
                    Description = item.Description;
                    DailyPrice = item.DailyPrice;
                    IsAvailable = item.IsAvailable;
                    Address = item.Address ?? string.Empty;
                    Latitude = item.Latitude;
                    Longitude = item.Longitude;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
                }
            }
            else if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Gets current device location
    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var hasPermission = await _locationService.CheckLocationPermissionAsync();
            if (!hasPermission)
            {
                hasPermission = await _locationService.RequestLocationPermissionAsync();
            }

            if (!hasPermission)
            {
                SetError("Location permission is required.");
                return;
            }

            var location = await _locationService.GetCurrentLocationAsync();
            if (location == null)
            {
                SetError("Unable to get current location.");
                return;
            }

            Latitude = location.Value.Latitude;
            Longitude = location.Value.Longitude;

            var address = await _locationService.GetAddressFromCoordinatesAsync(
                Latitude.Value, Longitude.Value);

            if (!string.IsNullOrEmpty(address))
            {
                Address = address;
            }
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

    /// @brief Saves the item
    [RelayCommand]
    private async Task SaveItemAsync()
    {
        if (IsBusy)
            return;

        // Validation
        if (string.IsNullOrWhiteSpace(Title))
        {
            SetError("Title is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            SetError("Description is required.");
            return;
        }

        if (DailyPrice <= 0)
        {
            SetError("Daily price must be greater than zero.");
            return;
        }

        if (SelectedCategory == null)
        {
            SetError("Please select a category.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            if (IsEditMode)
            {
                // Update existing item
                var success = await _itemService.UpdateItemAsync(
                    ItemId,
                    Title,
                    Description,
                    DailyPrice,
                    SelectedCategory.Id,
                    IsAvailable,
                    Latitude,
                    Longitude,
                    string.IsNullOrWhiteSpace(Address) ? null : Address);

                if (success)
                {
                    await _navigationService.NavigateBackAsync();
                }
                else
                {
                    SetError("Failed to update item.");
                }
            }
            else
            {
                // Create new item
                var item = await _itemService.CreateItemAsync(
                    Title,
                    Description,
                    DailyPrice,
                    SelectedCategory.Id,
                    Latitude,
                    Longitude,
                    string.IsNullOrWhiteSpace(Address) ? null : Address);

                if (item != null)
                {
                    await _navigationService.NavigateBackAsync();
                }
                else
                {
                    SetError("Failed to create item.");
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to save item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
