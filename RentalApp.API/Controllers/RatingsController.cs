using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Api.Controllers;

/// <summary>
/// Controller for managing ratings and reviews
/// Handles creating ratings and retrieving rating statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IRentalRepository _rentalRepository;

    public RatingsController(
        IRatingRepository ratingRepository,
        IRentalRepository rentalRepository)
    {
        _ratingRepository = ratingRepository;
        _rentalRepository = rentalRepository;
    }

    // GET: api/ratings/item/5
    /// <summary>
    /// Gets all ratings for a specific item
    /// </summary>
    [HttpGet("item/{itemId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RatingResponse>>> GetItemRatings(int itemId)
    {
        var ratings = await _ratingRepository.GetByItemIdAsync(itemId);

        var response = ratings.Select(MapToRatingResponse).ToList();

        return Ok(response);
    }

    // GET: api/ratings/item/5/stats
    /// <summary>
    /// Gets rating statistics for an item (average and count)
    /// </summary>
    [HttpGet("item/{itemId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<RatingStatsResponse>> GetItemRatingStats(int itemId)
    {
        var (average, count) = await _ratingRepository.GetItemRatingStatsAsync(itemId);

        return Ok(new RatingStatsResponse
        {
            AverageRating = average,
            TotalCount = count
        });
    }

    // GET: api/ratings/user/5
    /// <summary>
    /// Gets all ratings received by a specific user
    /// Useful for displaying user reputation
    /// </summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RatingResponse>>> GetUserRatings(int userId)
    {
        var ratings = await _ratingRepository.GetByUserIdAsync(userId);

        var response = ratings.Select(MapToRatingResponse).ToList();

        return Ok(response);
    }

    // GET: api/ratings/user/5/stats
    /// <summary>
    /// Gets rating statistics for a user (average and count)
    /// </summary>
    [HttpGet("user/{userId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<RatingStatsResponse>> GetUserRatingStats(int userId)
    {
        var average = await _ratingRepository.GetAverageRatingForUserAsync(userId);
        var ratings = await _ratingRepository.GetByUserIdAsync(userId);

        return Ok(new RatingStatsResponse
        {
            AverageRating = average,
            TotalCount = ratings.Count
        });
    }

    // GET: api/ratings/rental/5
    /// <summary>
    /// Gets the rating for a specific rental
    /// </summary>
    [HttpGet("rental/{rentalId}")]
    public async Task<ActionResult<RatingResponse>> GetRentalRating(int rentalId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rental = await _rentalRepository.GetByIdAsync(rentalId);

        if (rental == null)
            return NotFound("Rental not found");

        // User must be either the renter or the owner
        if (rental.RenterId != userId && rental.Item.OwnerId != userId)
            return Forbid();

        var rating = await _ratingRepository.GetByRentalIdAsync(rentalId);

        if (rating == null)
            return NotFound("No rating found for this rental");

        return Ok(MapToRatingResponse(rating));
    }

    // POST: api/ratings
    /// <summary>
    /// Creates a new rating/review for a completed rental
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RatingResponse>> CreateRating([FromBody] CreateRatingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Validate star rating
        if (request.Stars < 1 || request.Stars > 5)
            return BadRequest("Stars must be between 1 and 5");

        // Validate rental exists
        var rental = await _rentalRepository.GetByIdAsync(request.RentalId);

        if (rental == null)
            return NotFound("Rental not found");

        // Rental must be completed
        if (rental.Status != "Completed")
            return BadRequest("Can only rate completed rentals");

        // User must be the renter
        if (rental.RenterId != userId)
            return Forbid("Only the renter can rate the rental");

        // Check if rating already exists
        if (await _ratingRepository.RatingExistsForRentalAsync(request.RentalId))
            return BadRequest("Rating already exists for this rental");

        // Validate rated user
        if (request.RatedUserId != rental.Item.OwnerId)
            return BadRequest("Can only rate the item owner");

        var rating = new Rating
        {
            RentalId = request.RentalId,
            RaterId = userId,
            RatedUserId = request.RatedUserId,
            Stars = request.Stars,
            Comment = request.Comment,
            CreatedDate = DateTime.UtcNow
        };

        rating = await _ratingRepository.CreateAsync(rating);

        return CreatedAtAction(
            nameof(GetRentalRating),
            new { rentalId = rating.RentalId },
            MapToRatingResponse(rating));
    }

    // Helper method to map Rating to RatingResponse
    private static RatingResponse MapToRatingResponse(Rating rating)
    {
        return new RatingResponse
        {
            Id = rating.Id,
            RentalId = rating.RentalId,
            RaterId = rating.RaterId,
            RaterName = rating.Rater.FirstName + " " + rating.Rater.LastName,
            RatedUserId = rating.RatedUserId,
            RatedUserName = rating.RatedUser.FirstName + " " + rating.RatedUser.LastName,
            Stars = rating.Stars,
            Comment = rating.Comment,
            CreatedDate = rating.CreatedDate
        };
    }
}
