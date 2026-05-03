using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Service interface for rental-related operations
/// Communicates with the Rentals API endpoint
/// </summary>
public interface IRentalService
{
    /// <summary>
    /// Gets rentals where current user is the renter
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Active, Completed, Cancelled)</param>
    /// <returns>List of rentals</returns>
    Task<List<Rental>> GetMyRentalsAsync(string? status = null);

    /// <summary>
    /// Gets rentals for items owned by current user
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of rentals</returns>
    Task<List<Rental>> GetMyListingsAsync(string? status = null);

    /// <summary>
    /// Gets a single rental by ID
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <returns>Rental details or null if not found</returns>
    Task<Rental?> GetRentalByIdAsync(int rentalId);

    /// <summary>
    /// Creates a new rental request
    /// </summary>
    /// <param name="itemId">Item ID to rent</param>
    /// <param name="startDate">Rental start date</param>
    /// <param name="endDate">Rental end date</param>
    /// <param name="totalPrice">Total rental price</param>
    /// <returns>Created rental</returns>
    Task<Rental> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate, decimal totalPrice);

    /// <summary>
    /// Approves a pending rental request (owner only)
    /// </summary>
    /// <param name="rentalId">Rental ID to approve</param>
    /// <returns>True if successful</returns>
    Task<bool> ApproveRentalAsync(int rentalId);

    /// <summary>
    /// Completes an active rental (owner only)
    /// </summary>
    /// <param name="rentalId">Rental ID to complete</param>
    /// <returns>True if successful</returns>
    Task<bool> CompleteRentalAsync(int rentalId);

    /// <summary>
    /// Cancels a pending rental (renter or owner)
    /// </summary>
    /// <param name="rentalId">Rental ID to cancel</param>
    /// <returns>True if successful</returns>
    Task<bool> CancelRentalAsync(int rentalId);
}
