using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository implementation for Rental entity operations
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rental>> GetByRenterIdAsync(int renterId, string? status = null)
    {
        var query = _context.Rentals
            .Include(r => r.Item)
                .ThenInclude(i => i.Owner)
            .Include(r => r.Renter)
            .Where(r => r.RenterId == renterId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Rental>> GetByOwnerIdAsync(int ownerId, string? status = null)
    {
        var query = _context.Rentals
            .Include(r => r.Item)
                .ThenInclude(i => i.Owner)
            .Include(r => r.Renter)
            .Where(r => r.Item.OwnerId == ownerId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .Include(r => r.Item)
                .ThenInclude(i => i.Owner)
            .Include(r => r.Renter)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        // Reload navigation properties
        await _context.Entry(rental)
            .Reference(r => r.Item)
            .LoadAsync();
        await _context.Entry(rental.Item)
            .Reference(i => i.Owner)
            .LoadAsync();
        await _context.Entry(rental)
            .Reference(r => r.Renter)
            .LoadAsync();

        return rental;
    }

    public async Task UpdateAsync(Rental rental)
    {
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOverlappingRentalsAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        return await _context.Rentals
            .AnyAsync(r =>
                r.ItemId == itemId &&
                r.Status != "Cancelled" &&
                r.Status != "Completed" &&
                ((startDate >= r.StartDate && startDate < r.EndDate) ||
                 (endDate > r.StartDate && endDate <= r.EndDate) ||
                 (startDate <= r.StartDate && endDate >= r.EndDate)));
    }

    public async Task<Item?> GetItemWithOwnerAsync(int itemId)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == itemId);
    }
}
