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
/// View model for viewing and managing the current user's items
/// </summary>
public partial class MyItemsViewModel : BaseViewModel
{
    private readonly IItemService _itemService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Collection of user's items to display
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

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
    public MyItemsViewModel(IItemService itemService, INavigationService navigationService)
    {
        _itemService = itemService;
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

            var itemsList = await _itemService.GetMyItemsAsync();
            Items.Clear();

            foreach (var item in itemsList)
            {
                Items.Add(item);
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
    private async Task ViewItemAsync(Item item)
    {
        if (item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }

    /// <summary>
    /// Navigates to create new item page
    /// </summary>
    [RelayCommand]
    private async Task CreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }
}
