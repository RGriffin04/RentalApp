using System.Net.Http.Json;
using System.Text.Json;
using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// HTTP-based authentication service that calls the API instead of direct database access
/// </summary>
public class HttpAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private User? _currentUser;
    private List<string> _currentUserRoles = new();
    private string? _authToken;

    public event EventHandler<bool>? AuthenticationStateChanged;

    public HttpAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_authToken);

    public User? CurrentUser => _currentUser;

    public List<string> CurrentUserRoles => _currentUserRoles;

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new
            {
                email = email,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                ApiConfig.GetUrl("/auth/token"), 
                loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthenticationResult(false, "Invalid email or password");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (tokenResponse == null)
            {
                return new AuthenticationResult(false, "Invalid response from server");
            }

            _authToken = tokenResponse.Token;

            // Set authorization header for future requests
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            // Extract user info from token response (API returns user info with token)
            _currentUser = new User
            {
                Id = 0, // ID is in JWT claims, not needed for MAUI app
                Email = tokenResponse.Email ?? string.Empty,
                FirstName = tokenResponse.FirstName ?? string.Empty,
                LastName = tokenResponse.LastName ?? string.Empty,
                IsActive = true
            };

            _currentUserRoles = tokenResponse.Roles ?? new List<string>();

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var registerRequest = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                ApiConfig.GetUrl("/auth/register"), 
                registerRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                // Try to parse error message
                try
                {
                    using var doc = JsonDocument.Parse(errorContent);
                    if (doc.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        return new AuthenticationResult(false, messageElement.GetString() ?? "Registration failed");
                    }
                }
                catch { }

                return new AuthenticationResult(false, "Registration failed");
            }

            // After successful registration, automatically log in
            return await LoginAsync(email, password);
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;

        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public string? GetAuthToken()
    {
        return _authToken;
    }

    public bool HasRole(string roleName)
    {
        return _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyRole(params string[] roleNames)
    {
        return roleNames.Any(role => HasRole(role));
    }

    public bool HasAllRoles(params string[] roleNames)
    {
        return roleNames.All(role => HasRole(role));
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var changePasswordRequest = new
            {
                currentPassword = currentPassword,
                newPassword = newPassword
            };

            var response = await _httpClient.PostAsJsonAsync(
                ApiConfig.GetUrl("/auth/change-password"),
                changePasswordRequest);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // DTOs for API responses
    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
