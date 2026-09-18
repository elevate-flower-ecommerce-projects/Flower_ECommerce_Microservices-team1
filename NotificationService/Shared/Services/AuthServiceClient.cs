namespace NotificationService.Shared.Services;

// In NotificationService
public class AuthServiceClient
{
    private readonly HttpClient _httpClient;

    public AuthServiceClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<string>> GetDeviceTokensAsync(string userId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<string>>($"/api/users/{userId}/device-tokens");
        return response ?? new List<string>();
    }
}