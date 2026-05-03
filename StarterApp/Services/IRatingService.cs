using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Service interface for rating-related operations
/// Communicates with the Ratings API endpoint
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// Gets all ratings for a specific item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>List of ratings</returns>
    Task<List<Rating>> GetItemRatingsAsync(int itemId);

    /// <summary>
    /// Gets rating statistics for an item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>Tuple of (Average rating, Count)</returns>
    Task<(double Average, int Count)> GetItemRatingSummaryAsync(int itemId);

    /// <summary>
    /// Gets all ratings received by a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of ratings</returns>
    Task<List<Rating>> GetUserRatingsAsync(int userId);

    /// <summary>
    /// Gets rating statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Tuple of (Average rating, Count)</returns>
    Task<(double Average, int Count)> GetUserRatingSummaryAsync(int userId);

    /// <summary>
    /// Gets the rating for a specific rental
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <returns>Rating or null if not found</returns>
    Task<Rating?> GetRentalRatingAsync(int rentalId);

    /// <summary>
    /// Creates a new rating for a completed rental
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <param name="ratedUserId">User ID being rated (typically the item owner)</param>
    /// <param name="stars">Star rating (1-5)</param>
    /// <param name="comment">Optional comment</param>
    /// <returns>Created rating</returns>
    Task<Rating> CreateRatingAsync(int rentalId, int ratedUserId, int stars, string? comment = null);
}
