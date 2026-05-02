using RentalApp.Database.Models;

namespace RentalApp.Api.Repositories;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by email with roles loaded
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User with roles loaded, or null if not found</returns>
    Task<User?> GetByEmailWithRolesAsync(string email);

    /// <summary>
    /// Checks if a user with the given email already exists
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Gets the default role
    /// </summary>
    /// <returns>Default role or null if no default exists</returns>
    Task<Role?> GetDefaultRoleAsync();

    /// <summary>
    /// Adds a role to a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Task</returns>
    Task AddUserRoleAsync(int userId, int roleId);
}
