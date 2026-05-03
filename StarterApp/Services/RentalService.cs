using RentalApp.Database.Models;
using System.Web;

namespace RentalApp.Services;

/// <summary>
/// Service implementation for rental-related operations
/// Communicates with the Rentals API endpoint
/// </summary>
public class RentalService : BaseHttpService, IRentalService
{
    public RentalService(HttpClient httpClient, IAuthenticationService authService) 
        : base(httpClient, authService)
    {
    }

    public async Task<List<Rental>> GetMyRentalsAsync(string? status = null)
    {
        var endpoint = "/api/rentals/my";

        if (!string.IsNullOrWhiteSpace(status))
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["status"] = status;
            endpoint = $"{endpoint}?{query}";
        }

        var rentals = await GetAsync<List<Rental>>(endpoint);
        return rentals ?? new List<Rental>();
    }

    public async Task<List<Rental>> GetMyListingsAsync(string? status = null)
    {
        var endpoint = "/api/rentals/owner";

        if (!string.IsNullOrWhiteSpace(status))
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["status"] = status;
            endpoint = $"{endpoint}?{query}";
        }

        var rentals = await GetAsync<List<Rental>>(endpoint);
        return rentals ?? new List<Rental>();
    }

    public async Task<Rental?> GetRentalByIdAsync(int rentalId)
    {
        return await GetAsync<Rental>($"/api/rentals/{rentalId}");
    }

    public async Task<Rental> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate, decimal totalPrice)
    {
        var request = new
        {
            itemId,
            startDate,
            endDate,
            totalPrice
        };

        var rental = await PostAsync<object, Rental>("/api/rentals", request);
        return rental ?? throw new Exception("Failed to create rental");
    }

    public async Task<bool> ApproveRentalAsync(int rentalId)
    {
        return await PutAsync($"/api/rentals/{rentalId}/approve");
    }

    public async Task<bool> CompleteRentalAsync(int rentalId)
    {
        return await PutAsync($"/api/rentals/{rentalId}/complete");
    }

    public async Task<bool> CancelRentalAsync(int rentalId)
    {
        return await PutAsync($"/api/rentals/{rentalId}/cancel");
    }
}
