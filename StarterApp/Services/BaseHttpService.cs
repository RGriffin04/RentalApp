using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RentalApp.Services;

/// <summary>
/// Base service for HTTP API communication
/// Provides common HTTP methods with JSON serialization
/// </summary>
public abstract class BaseHttpService
{
    protected readonly HttpClient _httpClient;
    protected readonly IAuthenticationService _authService;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected BaseHttpService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(ApiConfig.BaseUrl);
        _httpClient.Timeout = ApiConfig.RequestTimeout;
    }

    /// <summary>
    /// Adds JWT token to request headers if user is authenticated
    /// </summary>
    protected void AddAuthorizationHeader()
    {
        var token = _authService.GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Performs GET request and deserializes response
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Performs POST request with JSON body
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AddAuthorizationHeader();
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            // Try to extract error message from response
            var errorContent = await response.Content.ReadAsStringAsync();

            // If the response is a simple string (like BadRequest returns), use it directly
            if (!string.IsNullOrWhiteSpace(errorContent) && 
                !errorContent.StartsWith("{") && 
                !errorContent.StartsWith("["))
            {
                throw new HttpRequestException(errorContent);
            }

            // Otherwise throw with status code
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseJson, JsonOptions);
    }

    /// <summary>
    /// Performs PUT request with JSON body
    /// </summary>
    protected async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
    {
        try
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Performs PUT request without body
    /// </summary>
    protected async Task<bool> PutAsync(string endpoint)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(string.Empty);
            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Performs DELETE request
    /// </summary>
    protected async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
