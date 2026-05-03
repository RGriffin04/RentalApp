using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository implementation for Item entity operations
/// Includes PostGIS spatial query support for location-based searches
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    // WGS84 (SRID 4326) is the standard for GPS coordinates
    private static readonly GeometryFactory _geometryFactory = 
        new GeometryFactory(new PrecisionModel(), 4326);

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetAllAsync(string? search = null, int? categoryId = null, bool? isAvailable = null)
    {
        var query = _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.Title.Contains(search) ||
                i.Description.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(i => i.IsAvailable == isAvailable.Value);
        }

        return await query
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Item>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Where(i => i.OwnerId == ownerId)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();
    }

    public async Task<Item> CreateAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // Reload navigation properties
        await _context.Entry(item)
            .Reference(i => i.Owner)
            .LoadAsync();
        await _context.Entry(item)
            .Reference(i => i.Category)
            .LoadAsync();

        return item;
    }

    public async Task UpdateAsync(Item item)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Item item)
    {
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    /// <summary>
    /// Gets items within a specified radius using PostGIS geography distance function
    /// PostGIS geography type uses meters for distance calculations
    /// </summary>
    public async Task<List<Item>> GetNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        // Create search point (PostGIS uses longitude, latitude order for Point constructor)
        var searchPoint = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var radiusMeters = radiusKm * 1000; // Convert km to meters for PostGIS

        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Include(i => i.ItemImages)
            .Where(i => i.IsAvailable && 
                        i.Location != null && 
                        i.Location.Distance(searchPoint) <= radiusMeters) // PostGIS distance in meters
            .OrderBy(i => i.Location.Distance(searchPoint))
            .ToListAsync();
    }

    /// <summary>
    /// Gets items within radius with distance information in kilometers
    /// Returns items ordered by distance with calculated distance values
    /// </summary>
    public async Task<List<(Item Item, double DistanceKm)>> GetNearbyWithDistanceAsync(
        double latitude, double longitude, double radiusKm)
    {
        var searchPoint = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var radiusMeters = radiusKm * 1000;

        var results = await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Include(i => i.ItemImages)
            .Where(i => i.IsAvailable && 
                        i.Location != null && 
                        i.Location.Distance(searchPoint) <= radiusMeters)
            .OrderBy(i => i.Location.Distance(searchPoint))
            .Select(i => new 
            {
                Item = i,
                DistanceKm = i.Location!.Distance(searchPoint) / 1000.0 // Convert meters to km
            })
            .ToListAsync();

        return results.Select(r => (r.Item, r.DistanceKm)).ToList();
    }

    /// <summary>
    /// Gets items with their average ratings and rating count
    /// Left join ensures items without ratings are included (with 0 average and 0 count)
    /// </summary>
    public async Task<List<(Item Item, double AverageRating, int RatingCount)>> GetWithRatingsAsync(
        string? search = null, int? categoryId = null, bool? isAvailable = null)
    {
        var query = _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.Title.Contains(search) ||
                i.Description.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(i => i.IsAvailable == isAvailable.Value);
        }

        var results = await query
            .GroupJoin(
                _context.Rentals.SelectMany(r => r.Ratings),
                item => item.Id,
                rating => rating.Rental.ItemId,
                (item, ratings) => new
                {
                    Item = item,
                    Ratings = ratings
                })
            .Select(x => new
            {
                x.Item,
                AverageRating = x.Ratings.Any() ? x.Ratings.Average(r => r.Stars) : 0.0,
                RatingCount = x.Ratings.Count()
            })
            .OrderByDescending(x => x.Item.CreatedDate)
            .ToListAsync();

        return results.Select(r => (r.Item, r.AverageRating, r.RatingCount)).ToList();
    }

    /// <summary>
    /// Helper method to create a PostGIS Point from latitude and longitude
    /// Note: PostGIS uses (longitude, latitude) order
    /// </summary>
    public static Point CreatePoint(double latitude, double longitude)
    {
        return _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }
}
