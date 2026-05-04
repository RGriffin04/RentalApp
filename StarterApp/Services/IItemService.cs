using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Service interface for item-related operations
/// Communicates with the Items API endpoint
/// </summary>
public interface IItemService
{
    /// <summary>
    /// Gets all items with optional filtering
    /// </summary>
    /// <param name="search">Search term for title or description</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="isAvailable">Filter by availability status</param>
    /// <returns>List of items</returns>
    Task<List<Item>> GetAllItemsAsync(string? search = null, int? categoryId = null, bool? isAvailable = null);

    /// <summary>
    /// Gets a single item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item details or null if not found</returns>
    Task<Item?> GetItemByIdAsync(int id);

    /// <summary>
    /// Gets items within a specified radius using location search
    /// </summary>
    /// <param name="latitude">Center latitude</param>
    /// <param name="longitude">Center longitude</param>
    /// <param name="radiusKm">Search radius in kilometers</param>
    /// <returns>List of items with distance information</returns>
    Task<List<ItemWithDistance>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm);

    /// <summary>
    /// Gets items owned by the current user
    /// </summary>
    /// <returns>List of user's items</returns>
    Task<List<Item>> GetMyItemsAsync();

    /// <summary>
    /// Creates a new item listing
    /// </summary>
    /// <param name="title">Item title</param>
    /// <param name="description">Item description</param>
    /// <param name="dailyPrice">Daily rental price</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="latitude">Optional latitude</param>
    /// <param name="longitude">Optional longitude</param>
    /// <param name="address">Optional address</param>
    /// <returns>Created item</returns>
    Task<Item> CreateItemAsync(
        string title, 
        string description, 
        decimal dailyPrice, 
        int categoryId,
        double? latitude = null,
        double? longitude = null,
        string? address = null);

    /// <summary>
    /// Updates an existing item
    /// </summary>
    /// <param name="itemId">Item ID to update</param>
    /// <param name="title">Updated title</param>
    /// <param name="description">Updated description</param>
    /// <param name="dailyPrice">Updated daily price</param>
    /// <param name="categoryId">Updated category ID</param>
    /// <param name="isAvailable">Updated availability</param>
    /// <param name="latitude">Updated latitude</param>
    /// <param name="longitude">Updated longitude</param>
    /// <param name="address">Updated address</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateItemAsync(
        int itemId,
        string? title = null,
        string? description = null,
        decimal? dailyPrice = null,
        int? categoryId = null,
        bool? isAvailable = null,
        double? latitude = null,
        double? longitude = null,
        string? address = null);

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="itemId">Item ID to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteItemAsync(int itemId);

    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <returns>List of categories</returns>
    Task<List<Category>> GetCategoriesAsync();
}

/// <summary>
/// Helper class to represent an item with distance information
/// </summary>
public class ItemWithDistance
{
    public Item Item { get; set; } = null!;
    public double DistanceKm { get; set; }
}

/// <summary>
/// DTO for deserializing the API response from /api/items/nearby
/// Matches the flattened structure returned by ItemWithDistanceResponse
/// </summary>
internal class ItemWithDistanceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public double DistanceKm { get; set; }
}
