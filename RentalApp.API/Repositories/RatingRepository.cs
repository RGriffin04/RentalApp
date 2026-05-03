using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository implementation for Rating entity operations
/// Handles review/rating CRUD and statistical calculations
/// </summary>
public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all ratings for an item by joining through rentals
    /// Includes rater and rental information for display
    /// </summary>
    public async Task<List<Rating>> GetByItemIdAsync(int itemId)
    {
        return await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .Include(r => r.Rental)
                .ThenInclude(rental => rental.Item)
            .Where(r => r.Rental.ItemId == itemId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all ratings where the specified user was rated
    /// Useful for calculating user reputation
    /// </summary>
    public async Task<List<Rating>> GetByUserIdAsync(int userId)
    {
        return await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .Include(r => r.Rental)
            .Where(r => r.RatedUserId == userId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the rating for a specific rental
    /// Uses unique index on RentalId to ensure one rating per rental
    /// </summary>
    public async Task<Rating?> GetByRentalIdAsync(int rentalId)
    {
        return await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .Include(r => r.Rental)
            .FirstOrDefaultAsync(r => r.RentalId == rentalId);
    }

    /// <summary>
    /// Creates a new rating
    /// Navigation properties are reloaded after save
    /// </summary>
    public async Task<Rating> CreateAsync(Rating rating)
    {
        rating.CreatedDate = DateTime.UtcNow;
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Reload navigation properties
        await _context.Entry(rating)
            .Reference(r => r.Rater)
            .LoadAsync();
        await _context.Entry(rating)
            .Reference(r => r.RatedUser)
            .LoadAsync();
        await _context.Entry(rating)
            .Reference(r => r.Rental)
            .LoadAsync();

        return rating;
    }

    /// <summary>
    /// Calculates average rating for an item
    /// Joins through rentals to find all ratings for the item
    /// </summary>
    public async Task<double> GetAverageRatingForItemAsync(int itemId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.Rental)
            .Where(r => r.Rental.ItemId == itemId)
            .Select(r => r.Stars)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0.0;
    }

    /// <summary>
    /// Calculates average rating received by a user
    /// Based on ratings where user is the RatedUserId
    /// </summary>
    public async Task<double> GetAverageRatingForUserAsync(int userId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.RatedUserId == userId)
            .Select(r => r.Stars)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0.0;
    }

    /// <summary>
    /// Gets both average and count for an item in a single query
    /// More efficient than calling separate methods
    /// </summary>
    public async Task<(double Average, int Count)> GetItemRatingStatsAsync(int itemId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.Rental)
            .Where(r => r.Rental.ItemId == itemId)
            .Select(r => r.Stars)
            .ToListAsync();

        if (!ratings.Any())
        {
            return (0.0, 0);
        }

        return (ratings.Average(), ratings.Count);
    }

    /// <summary>
    /// Checks if a rating already exists for a rental
    /// Prevents duplicate ratings for the same rental
    /// </summary>
    public async Task<bool> RatingExistsForRentalAsync(int rentalId)
    {
        return await _context.Ratings.AnyAsync(r => r.RentalId == rentalId);
    }
}
