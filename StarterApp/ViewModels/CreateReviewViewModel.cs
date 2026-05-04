/// @file CreateReviewViewModel.cs
/// @brief View model for creating a review/rating for a completed rental
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// @brief View model for creating reviews
/// @details Allows users to rate and comment on completed rentals
/// @extends BaseViewModel
[QueryProperty(nameof(RentalId), "rentalId")]
public partial class CreateReviewViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IRatingService _ratingService;
    private readonly INavigationService _navigationService;

    /// @brief The ID of the rental to review
    [ObservableProperty]
    private int rentalId;

    /// @brief The rental details
    [ObservableProperty]
    private Rental? rental;

    /// @brief Star rating (1-5)
    [ObservableProperty]
    private int stars = 5;

    /// @brief Review comment
    [ObservableProperty]
    private string comment = string.Empty;

    /// @brief Whether a rating already exists for this rental
    [ObservableProperty]
    private bool hasExistingRating;

    /// @brief Default constructor for design-time support
    public CreateReviewViewModel()
    {
        Title = "Leave Review";
    }

    /// <summary>
    /// Initializes a new instance of the CreateReviewViewModel class
    /// </summary>
    public CreateReviewViewModel(
        IRentalService rentalService,
        IRatingService ratingService,
        INavigationService navigationService)
    {
        _rentalService = rentalService;
        _ratingService = ratingService;
        _navigationService = navigationService;
        Title = "Leave Review";
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

    /// @brief Loads rental details and checks for existing rating
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

            Title = $"Review: {Rental.ItemTitle}";

            // Check if a rating already exists
            var existingRating = await _ratingService.GetRentalRatingAsync(RentalId);
            HasExistingRating = existingRating != null;

            if (HasExistingRating)
            {
                SetError("You have already reviewed this rental.");
            }
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

    /// @brief Submits the review
    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Validate
            if (Stars < 1 || Stars > 5)
            {
                SetError("Please select a rating between 1 and 5 stars.");
                return;
            }

            if (Rental == null)
            {
                SetError("Rental information not loaded.");
                return;
            }

            // Determine who to rate (the item owner)
            if (!Rental.OwnerId.HasValue)
            {
                SetError("Unable to determine the item owner.");
                return;
            }

            // Submit rating
            await _ratingService.CreateRatingAsync(
                RentalId,
                Rental.OwnerId.Value,
                Stars,
                string.IsNullOrWhiteSpace(Comment) ? null : Comment);

            // Navigate back
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit review: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
