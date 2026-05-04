/// @file MyItemsViewModel.cs
/// @brief View model for the my items page
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// <summary>
/// Wrapper class to hold an item and its associated rental requests
/// </summary>
public partial class ItemWithRentals : ObservableObject
{
    public Item? Item { get; set; }

    [ObservableProperty]
    private ObservableCollection<Rental> _rentals = new();

    public int PendingRentalsCount => _rentals.Count(r => r.Status == "Pending");
    public int TotalRentalsCount => _rentals.Count;
}

/// <summary>
/// View model for viewing and managing the current user's items
/// </summary>
public partial class MyItemsViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Collection of user's items with their rental requests
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ItemWithRentals> _itemsWithRentals = new();

    /// <summary>
    /// Whether the list is currently being refreshed
    /// </summary>
    [ObservableProperty]
    private bool _isRefreshing;

    /// <summary>
    /// Default constructor for design-time support
    /// </summary>
    public MyItemsViewModel()
    {
        Title = "My Items";
    }

    /// <summary>
    /// Initializes a new instance of the MyItemsViewModel class
    /// </summary>
    public MyItemsViewModel(IItemService itemService, IRentalService rentalService, INavigationService navigationService)
    {
        _itemService = itemService;
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "My Items";
    }

    /// <summary>
    /// Loads the user's items when the page appears
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load user's items
            var itemsList = await _itemService.GetMyItemsAsync();

            // Load all rental requests for items owned by user
            var allRentals = await _rentalService.GetMyListingsAsync();

            ItemsWithRentals.Clear();

            foreach (var item in itemsList)
            {
                var itemWithRentals = new ItemWithRentals
                {
                    Item = item
                };

                // Add rentals for this item
                var itemRentals = allRentals.Where(r => r.ItemId == item.Id).ToList();
                foreach (var rental in itemRentals)
                {
                    itemWithRentals.Rentals.Add(rental);
                }

                ItemsWithRentals.Add(itemWithRentals);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load your items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Handles pull-to-refresh
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Navigates to item detail page for viewing/editing
    /// </summary>
    [RelayCommand]
    private async Task ViewItemAsync(ItemWithRentals itemWithRentals)
    {
        if (itemWithRentals?.Item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={itemWithRentals.Item.Id}");
    }

    /// <summary>
    /// Navigates to create new item page
    /// </summary>
    [RelayCommand]
    private async Task CreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    /// <summary>
    /// Approves a pending rental request
    /// </summary>
    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        if (rental == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _rentalService.ApproveRentalAsync(rental.Id);

            if (success)
            {
                await LoadDataAsync();
            }
            else
            {
                SetError("Failed to approve rental request.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to approve rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Rejects (cancels) a pending rental request
    /// </summary>
    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        if (rental == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _rentalService.CancelRentalAsync(rental.Id);

            if (success)
            {
                await LoadDataAsync();
            }
            else
            {
                SetError("Failed to reject rental request.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to reject rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Completes an active rental
    /// </summary>
    [RelayCommand]
    private async Task CompleteRentalAsync(Rental rental)
    {
        if (rental == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _rentalService.CompleteRentalAsync(rental.Id);

            if (success)
            {
                await LoadDataAsync();
            }
            else
            {
                SetError("Failed to complete rental.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to complete rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates to edit item page
    /// </summary>
    [RelayCommand]
    private async Task EditItemAsync(ItemWithRentals itemWithRentals)
    {
        if (itemWithRentals?.Item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={itemWithRentals.Item.Id}");
    }

    /// <summary>
    /// Deletes an item after confirmation
    /// </summary>
    [RelayCommand]
    private async Task DeleteItemAsync(ItemWithRentals itemWithRentals)
    {
        if (itemWithRentals?.Item == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _itemService.DeleteItemAsync(itemWithRentals.Item.Id);

            if (success)
            {
                // Remove from collection
                ItemsWithRentals.Remove(itemWithRentals);
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
}
