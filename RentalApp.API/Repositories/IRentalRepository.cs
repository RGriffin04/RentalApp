using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository interface for Rental entity operations
/// </summary>
public interface IRentalRepository
{
    /// <summary>
    /// Gets all rentals for a specific renter
    /// </summary>
    /// <param name="renterId">User ID of the renter</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of rentals with navigation properties loaded</returns>
    Task<List<Rental>> GetByRenterIdAsync(int renterId, string? status = null);

    /// <summary>
    /// Gets all rentals for items owned by a specific user
    /// </summary>
    /// <param name="ownerId">User ID of the item owner</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of rentals with navigation properties loaded</returns>
    Task<List<Rental>> GetByOwnerIdAsync(int ownerId, string? status = null);

    /// <summary>
    /// Gets a single rental by ID with all navigation properties
    /// </summary>
    /// <param name="id">Rental ID</param>
    /// <returns>Rental with navigation properties loaded, or null if not found</returns>
    Task<Rental?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new rental
    /// </summary>
    /// <param name="rental">Rental to create</param>
    /// <returns>Created rental with navigation properties loaded</returns>
    Task<Rental> CreateAsync(Rental rental);

    /// <summary>
    /// Updates an existing rental
    /// </summary>
    /// <param name="rental">Rental with updated values</param>
    /// <returns>Task</returns>
    Task UpdateAsync(Rental rental);

    /// <summary>
    /// Checks if there is a date overlap with existing active rentals for an item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="startDate">Requested start date</param>
    /// <param name="endDate">Requested end date</param>
    /// <returns>True if there is an overlap, false otherwise</returns>
    Task<bool> HasOverlappingRentalsAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets an item by ID with owner navigation property loaded
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>Item with owner loaded, or null if not found</returns>
    Task<Item?> GetItemWithOwnerAsync(int itemId);
}
