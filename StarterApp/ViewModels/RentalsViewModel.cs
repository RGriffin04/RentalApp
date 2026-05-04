/// @file RentalsViewModel.cs
/// @brief View model for rentals page
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// @brief View model for managing rentals as renter and owner
/// @extends BaseViewModel
public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IRatingService _ratingService;
    private readonly INavigationService _navigationService;

    /// @brief Collection of rentals where user is the renter
    [ObservableProperty]
    private ObservableCollection<Rental> _myRentals = new();

    /// @brief Whether data is being refreshed
    [ObservableProperty]
    private bool _isRefreshing;

    /// @brief Default constructor for design-time support
    public RentalsViewModel()
    {
        Title = "Rentals";
    }

    /// @brief Initializes a new instance of the RentalsViewModel class
    public RentalsViewModel(
        IRentalService rentalService,
        IRatingService ratingService,
        INavigationService navigationService)
    {
        _rentalService = rentalService;
        _ratingService = ratingService;
        _navigationService = navigationService;
        Title = "Rentals";
    }

    /// @brief Loads all rental data
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            await ReloadDataInternalAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Internal method to reload data without IsBusy check
    private async Task ReloadDataInternalAsync()
    {
        // Load rentals as renter
        var myRentals = await _rentalService.GetMyRentalsAsync();
        MyRentals.Clear();
        foreach (var rental in myRentals)
        {
            MyRentals.Add(rental);
        }
    }

    /// @brief Refreshes the rental list
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing)
            return;

        try
        {
            IsRefreshing = true;
            ClearError();
            await LoadDataAsync();
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

    /// @brief Cancels a rental
    [RelayCommand]
    private async Task CancelRentalAsync(Rental rental)
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
                await ReloadDataInternalAsync();
            }
            else
            {
                SetError("Failed to cancel rental.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to cancel rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// @brief Navigates to leave a review
    [RelayCommand]
    private async Task LeaveReviewAsync(Rental rental)
    {
        if (rental == null)
            return;

        await _navigationService.NavigateToAsync($"CreateReviewPage?rentalId={rental.Id}");
    }

    /// @brief Views rental details
    [RelayCommand]
    private async Task ViewRentalAsync(Rental rental)
    {
        if (rental == null)
            return;

        await _navigationService.NavigateToAsync($"RentalDetailPage?rentalId={rental.Id}");
    }
}
