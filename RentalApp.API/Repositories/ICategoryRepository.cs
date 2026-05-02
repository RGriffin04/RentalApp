using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository interface for Category entity operations
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets all categories ordered by name
    /// </summary>
    /// <returns>List of all categories</returns>
    Task<List<Category>> GetAllAsync();

    /// <summary>
    /// Gets a single category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category or null if not found</returns>
    Task<Category?> GetByIdAsync(int id);
}
