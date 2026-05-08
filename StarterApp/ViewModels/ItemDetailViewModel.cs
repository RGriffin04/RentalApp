/// @file ItemDetailViewModel.cs
/// @brief View model for the item detail page
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// @brief View model for viewing item details and initiating rentals
/// @details Displays full item information, ratings, and allows rental requests
/// @extends BaseViewModel
[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly IRatingService _ratingService;
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    /// @brief The ID of the item to display
    [ObservableProperty]
    private int _itemId;

    /// @brief The item details
    [ObservableProperty]
    private Item? _item;

    /// @brief Average rating for the item
    [ObservableProperty]
    private double _averageRating;
    
    /// @brief Number of ratings
    [ObservableProperty]
    private int _ratingCount;

    /// @brief Whether the current user owns this item
    [ObservableProperty]
    private bool _isOwner;

    /// @brief Whether the item can be rented by the current user
    [ObservableProperty]
    private bool _canRent;

    /// @brief Start date for rental
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    /// @brief End date for rental
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(2);

    /// @brief Calculated total price
    [ObservableProperty]
    private decimal _totalPrice;

    /// @brief Number of rental days
    [ObservableProperty]
    private int _rentalDays;

    /// @brief Default constructor for design-time support
    public ItemDetailViewModel()
    {
        Title = "Item Details";
    }

    /// <summary>
    /// Initializes a new instance of the ItemDetailViewModel class
    /// </summary>
    public ItemDetailViewModel(
        IItemService itemService,
        IRatingService ratingService,
        IRentalService rentalService,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemService = itemService;
        _ratingService = ratingService;
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    /// @brief Loads the item details
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (ItemId > 0)
        {
            await LoadItemAsync();
        }
    }

    /// @brief Loads item details and ratings
    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load item
            Item = await _itemService.GetItemByIdAsync(ItemId);

            if (Item == null)
            {
                SetError("Item not found.");
                return;
            }

            Title = Item.Title;

            // Load ratings
            var ratingSummary = await _ratingService.GetItemRatingSummaryAsync(ItemId);
            AverageRating = ratingSummary.Average;
            RatingCount = ratingSummary.Count;

            // Check ownership
            IsOwner = _authService.CurrentUser?.Id == Item.OwnerId;
            CanRent = !IsOwner && Item.IsAvailable && _authService.IsAuthenticated;

            // Calculate initial price
            CalculateTotalPrice();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Calculates total rental price based on dates
    partial void OnStartDateChanged(DateTime value)
    {
        CalculateTotalPrice();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        CalculateTotalPrice();
    }

    private void CalculateTotalPrice()
    {
        if (Item == null || EndDate <= StartDate)
        {
            RentalDays = 0;
            TotalPrice = 0;
            return;
        }

        RentalDays = (EndDate - StartDate).Days;
        TotalPrice = Item.DailyPrice * RentalDays;
    }

    /// @brief Creates a rental request for the item
    [RelayCommand]
    private async Task RentItemAsync()
    {
        if (Item == null || !CanRent)
            return;

        if (StartDate < DateTime.Today)
        {
            SetError("Start date cannot be in the past.");
            return;
        }

        if (EndDate <= StartDate)
        {
            SetError("End date must be after start date.");
            return;
        }

        // Check if user is trying to rent their own item
        if (_authService.CurrentUser != null && Item.OwnerId == _authService.CurrentUser.Id)
        {
            SetError("You cannot rent your own item. You already own this!");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            // Create rental request
            var rental = await _rentalService.CreateRentalAsync(
                ItemId, 
                StartDate, 
                EndDate, 
                TotalPrice);

            // Navigate to rentals page to see the new rental
            await _navigationService.NavigateToAsync("//RentalsPage");
        }
        catch (HttpRequestException httpEx)
        {
            // Display the actual error message from the API
            SetError(httpEx.Message);
        }
        catch (Exception ex)
        {
            // Fallback for unexpected errors
            SetError($"Unable to create rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Navigates to edit item page
    [RelayCommand]
    private async Task EditItemAsync()
    {
        if (Item == null || !IsOwner)
            return;

        await _navigationService.NavigateToAsync($"EditItemPage?itemId={ItemId}");
    }

    /// @brief Deletes the item
    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (Item == null || !IsOwner)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _itemService.DeleteItemAsync(ItemId);

            if (success)
            {
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError("Failed to delete item. It may have active rentals.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Views ratings for this item
    [RelayCommand]
    private async Task ViewRatingsAsync()
    {
        if (Item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemRatingsPage?itemId={ItemId}");
    }
}
