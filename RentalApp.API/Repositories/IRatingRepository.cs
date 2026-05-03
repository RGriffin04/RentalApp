using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository interface for Rating entity operations
/// Handles review/rating CRUD operations and rating statistics
/// </summary>
public interface IRatingRepository
{
    /// <summary>
    /// Gets all ratings for a specific item
    /// Includes rater information for display
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>List of ratings with navigation properties loaded</returns>
    Task<List<Rating>> GetByItemIdAsync(int itemId);

    /// <summary>
    /// Gets all ratings received by a specific user
    /// Useful for showing a user's reputation
    /// </summary>
    /// <param name="userId">User ID of the rated user</param>
    /// <returns>List of ratings where user was rated</returns>
    Task<List<Rating>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets the rating for a specific rental
    /// Each rental should only have one rating
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <returns>Rating for the rental, or null if not found</returns>
    Task<Rating?> GetByRentalIdAsync(int rentalId);

    /// <summary>
    /// Creates a new rating/review
    /// </summary>
    /// <param name="rating">Rating to create</param>
    /// <returns>Created rating with navigation properties loaded</returns>
    Task<Rating> CreateAsync(Rating rating);

    /// <summary>
    /// Calculates the average rating for an item
    /// Based on all ratings for rentals of that item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>Average star rating (0.0 if no ratings)</returns>
    Task<double> GetAverageRatingForItemAsync(int itemId);

    /// <summary>
    /// Calculates the average rating for a user
    /// Based on all ratings where the user was rated
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Average star rating (0.0 if no ratings)</returns>
    Task<double> GetAverageRatingForUserAsync(int userId);

    /// <summary>
    /// Gets rating statistics for an item (average and count)
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>Tuple of (Average, Count)</returns>
    Task<(double Average, int Count)> GetItemRatingStatsAsync(int itemId);

    /// <summary>
    /// Checks if a rating already exists for a rental
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <returns>True if rating exists, false otherwise</returns>
    Task<bool> RatingExistsForRentalAsync(int rentalId);
}
