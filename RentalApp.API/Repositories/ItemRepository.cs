using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository implementation for Item entity operations
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

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
}
