using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository interface for Item entity operations
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Gets all items with optional filtering
    /// </summary>
    /// <param name="search">Search term for title or description</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="isAvailable">Filter by availability status</param>
    /// <returns>List of items with owner and category information loaded</returns>
    Task<List<Item>> GetAllAsync(string? search = null, int? categoryId = null, bool? isAvailable = null);

    /// <summary>
    /// Gets a single item by ID with navigation properties loaded
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item with owner and category loaded, or null if not found</returns>
    Task<Item?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all items owned by a specific user
    /// </summary>
    /// <param name="ownerId">User ID of the owner</param>
    /// <returns>List of items owned by the user with owner and category loaded</returns>
    Task<List<Item>> GetByOwnerIdAsync(int ownerId);

    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <param name="item">Item to create</param>
    /// <returns>Created item with navigation properties loaded</returns>
    Task<Item> CreateAsync(Item item);

    /// <summary>
    /// Updates an existing item
    /// </summary>
    /// <param name="item">Item with updated values</param>
    /// <returns>Task</returns>
    Task UpdateAsync(Item item);

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="item">Item to delete</param>
    /// <returns>Task</returns>
    Task DeleteAsync(Item item);

    /// <summary>
    /// Checks if a category exists
    /// </summary>
    /// <param name="categoryId">Category ID to check</param>
    /// <returns>True if category exists, false otherwise</returns>
    Task<bool> CategoryExistsAsync(int categoryId);
}
