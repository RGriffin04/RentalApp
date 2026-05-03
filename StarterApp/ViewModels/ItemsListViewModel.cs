/// @file ItemsListViewModel.cs
/// @brief View model for the items list page
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// @brief View model for browsing marketplace items
/// @details Manages item listing, filtering, and navigation to item details
/// @extends BaseViewModel
public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly INavigationService _navigationService;

    /// @brief Collection of items to display
    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

    /// @brief Collection of categories for filtering
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    /// @brief Search text for filtering items
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// @brief Selected category for filtering
    [ObservableProperty]
    private Category? _selectedCategory;

    /// @brief Whether to show only available items
    [ObservableProperty]
    private bool _showOnlyAvailable = true;

    /// @brief Whether the list is currently being refreshed
    [ObservableProperty]
    private bool _isRefreshing;

    /// @brief Default constructor for design-time support
    public ItemsListViewModel()
    {
        Title = "Browse Items";
    }

    /// @brief Initializes a new instance of the ItemsListViewModel class
    /// @param itemService The item service instance
    /// @param navigationService The navigation service instance
    public ItemsListViewModel(IItemService itemService, INavigationService navigationService)
    {
        _itemService = itemService;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    /// @brief Loads items and categories when the page appears
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load categories first time only
            if (Categories.Count == 0)
            {
                var categoriesList = await _itemService.GetCategoriesAsync();
                Categories.Clear();

                // Add "All Categories" option
                Categories.Add(new Category { Id = 0, Name = "All Categories" });

                foreach (var category in categoriesList)
                {
                    Categories.Add(category);
                }

                SelectedCategory = Categories[0];
            }

            // Load items
            await LoadItemsAsync();
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

    /// @brief Loads items based on current filters
    private async Task LoadItemsAsync()
    {
        var categoryId = SelectedCategory?.Id > 0 ? (int?)SelectedCategory.Id : null;
        var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

        var itemsList = await _itemService.GetAllItemsAsync(
            search: search,
            categoryId: categoryId,
            isAvailable: ShowOnlyAvailable ? true : null);

        Items.Clear();
        foreach (var item in itemsList)
        {
            Items.Add(item);
        }

        // Don't show error messages for empty results - the EmptyView in XAML handles this
    }

    /// @brief Refreshes the item list
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing)
            return;

        try
        {
            IsRefreshing = true;
            ClearError();
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to refresh: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// @brief Applies the current filters and reloads items
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to apply filters: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Navigates to item detail page
    /// @param item The item to view details for
    [RelayCommand]
    private async Task ViewItemAsync(Item item)
    {
        if (item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }

    /// @brief Navigates to nearby items search page
    [RelayCommand]
    private async Task GoToNearbySearchAsync()
    {
        await _navigationService.NavigateToAsync("NearbyItemsPage");
    }

    /// @brief Navigates to create item page
    [RelayCommand]
    private async Task CreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    /// @brief Navigates to my items page
    [RelayCommand]
    private async Task GoToMyItemsAsync()
    {
        await _navigationService.NavigateToAsync("MyItemsPage");
    }

    /// @brief Navigates to rentals page
    [RelayCommand]
    private async Task GoToRentalsAsync()
    {
        await _navigationService.NavigateToAsync("RentalsPage");
    }
}
