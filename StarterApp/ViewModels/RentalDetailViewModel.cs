/// @file RentalDetailViewModel.cs
/// @brief View model for displaying rental details
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// @brief View model for viewing rental details
/// @details Displays full rental information including item, dates, status, and pricing
/// @extends BaseViewModel
[QueryProperty(nameof(RentalId), "rentalId")]
public partial class RentalDetailViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    /// @brief The ID of the rental to display
    [ObservableProperty]
    private int rentalId;

    /// @brief The rental details
    [ObservableProperty]
    private Rental? rental;

    /// @brief Number of rental days
    [ObservableProperty]
    private int rentalDays;

    /// @brief Default constructor for design-time support
    public RentalDetailViewModel()
    {
        Title = "Rental Details";
    }

    /// <summary>
    /// Initializes a new instance of the RentalDetailViewModel class
    /// </summary>
    public RentalDetailViewModel(
        IRentalService rentalService,
        INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "Rental Details";
    }

    /// @brief Loads the rental details
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (RentalId > 0)
        {
            await LoadRentalAsync();
        }
    }

    /// @brief Loads rental details
    private async Task LoadRentalAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load rental
            Rental = await _rentalService.GetRentalByIdAsync(RentalId);

            if (Rental == null)
            {
                SetError("Rental not found.");
                return;
            }

            Title = $"Rental: {Rental.ItemTitle}";

            // Calculate rental days
            RentalDays = (Rental.EndDate - Rental.StartDate).Days;
            if (RentalDays < 1)
                RentalDays = 1;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
