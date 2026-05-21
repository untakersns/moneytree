using MoneyTreeFront.DTOs;

namespace MoneyTreeFront.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;

    public AuthService(
        HttpClient httpClient,
        LocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка входа: {error}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse != null)
        {
            // Сохраняем токены
            await _localStorage.SetItemAsync("accessToken", authResponse.AccessToken);
            await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
        }

        return authResponse;
    }

    public async Task<AuthResponse?> RegisterAsync(string email, string password, string currency = "RUB")
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new { email, password, currency });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка регистрации: {error}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse != null)
        {
            // Сохраняем токены
            await _localStorage.SetItemAsync("accessToken", authResponse.AccessToken);
            await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
        }

        return authResponse;
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync("refreshToken");

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _httpClient.PostAsJsonAsync("/api/auth/logout", new { refreshToken });
            }
        }
        catch
        {
            // Игнорируем ошибки при logout
        }
        finally
        {
            // Очищаем токены
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync("accessToken");
        return !string.IsNullOrEmpty(token);
    }
}