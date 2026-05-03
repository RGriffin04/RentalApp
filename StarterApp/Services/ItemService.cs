using RentalApp.Database.Models;
using System.Web;

namespace RentalApp.Services;

/// <summary>
/// Service implementation for item-related operations
/// Communicates with the Items API endpoint
/// </summary>
public class ItemService : BaseHttpService, IItemService
{
    public ItemService(HttpClient httpClient, IAuthenticationService authService) 
        : base(httpClient, authService)
    {
    }

    public async Task<List<Item>> GetAllItemsAsync(string? search = null, int? categoryId = null, bool? isAvailable = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrWhiteSpace(search))
            query["search"] = search;

        if (categoryId.HasValue)
            query["categoryId"] = categoryId.Value.ToString();

        if (isAvailable.HasValue)
            query["isAvailable"] = isAvailable.Value.ToString();

        var queryString = query.ToString();
        var endpoint = string.IsNullOrEmpty(queryString) 
            ? "/api/items" 
            : $"/api/items?{queryString}";

        var items = await GetAsync<List<Item>>(endpoint);
        return items ?? new List<Item>();
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await GetAsync<Item>($"/api/items/{id}");
    }

    public async Task<List<ItemWithDistance>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm)
    {
        var endpoint = $"/api/items/nearby?lat={latitude}&lon={longitude}&radius={radiusKm}";
        var items = await GetAsync<List<ItemWithDistance>>(endpoint);
        return items ?? new List<ItemWithDistance>();
    }

    public async Task<List<Item>> GetMyItemsAsync()
    {
        var items = await GetAsync<List<Item>>("/api/items/my");
        return items ?? new List<Item>();
    }

    public async Task<Item> CreateItemAsync(
        string title, 
        string description, 
        decimal dailyPrice, 
        int categoryId,
        double? latitude = null,
        double? longitude = null,
        string? address = null)
    {
        var request = new
        {
            title,
            description,
            dailyPrice,
            categoryId,
            isAvailable = true,
            latitude,
            longitude,
            address
        };

        var item = await PostAsync<object, Item>("/api/items", request);
        return item ?? throw new Exception("Failed to create item");
    }

    public async Task<bool> UpdateItemAsync(
        int itemId,
        string? title = null,
        string? description = null,
        decimal? dailyPrice = null,
        int? categoryId = null,
        bool? isAvailable = null,
        double? latitude = null,
        double? longitude = null,
        string? address = null)
    {
        var request = new
        {
            title,
            description,
            dailyPrice,
            categoryId,
            isAvailable,
            latitude,
            longitude,
            address
        };

        return await PutAsync($"/api/items/{itemId}", request);
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        return await DeleteAsync($"/api/items/{itemId}");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await GetAsync<List<Category>>("/api/categories");
        return categories ?? new List<Category>();
    }
}
