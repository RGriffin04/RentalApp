using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Service implementation for rating-related operations
/// Communicates with the Ratings API endpoint
/// </summary>
public class RatingService : BaseHttpService, IRatingService
{
    public RatingService(HttpClient httpClient, IAuthenticationService authService) 
        : base(httpClient, authService)
    {
    }

    public async Task<List<Rating>> GetItemRatingsAsync(int itemId)
    {
        var ratings = await GetAsync<List<Rating>>($"/api/ratings/item/{itemId}");
        return ratings ?? new List<Rating>();
    }

    public async Task<(double Average, int Count)> GetItemRatingSummaryAsync(int itemId)
    {
        var stats = await GetAsync<RatingStats>($"/api/ratings/item/{itemId}/stats");
        return stats != null ? (stats.AverageRating, stats.TotalCount) : (0.0, 0);
    }

    public async Task<List<Rating>> GetUserRatingsAsync(int userId)
    {
        var ratings = await GetAsync<List<Rating>>($"/api/ratings/user/{userId}");
        return ratings ?? new List<Rating>();
    }

    public async Task<(double Average, int Count)> GetUserRatingSummaryAsync(int userId)
    {
        var stats = await GetAsync<RatingStats>($"/api/ratings/user/{userId}/stats");
        return stats != null ? (stats.AverageRating, stats.TotalCount) : (0.0, 0);
    }

    public async Task<Rating?> GetRentalRatingAsync(int rentalId)
    {
        return await GetAsync<Rating>($"/api/ratings/rental/{rentalId}");
    }

    public async Task<Rating> CreateRatingAsync(int rentalId, int ratedUserId, int stars, string? comment = null)
    {
        var request = new
        {
            rentalId,
            ratedUserId,
            stars,
            comment
        };

        var rating = await PostAsync<object, Rating>("/api/ratings", request);
        return rating ?? throw new Exception("Failed to create rating");
    }

    /// <summary>
    /// Helper class for deserializing rating statistics
    /// </summary>
    private class RatingStats
    {
        public double AverageRating { get; set; }
        public int TotalCount { get; set; }
    }
}
