using System.Text.Json;
using MoneyTreeFront.DTOs;

namespace MoneyTreeFront.Services;

/// <summary>
/// Сервис для проверки и обновления JWT токена
/// </summary>
public class TokenRefreshService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LocalStorageService _localStorage;

    public TokenRefreshService(
        IHttpClientFactory httpClientFactory,
        LocalStorageService localStorage)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
    }

    /// <summary>
    /// Проверяет, истёк ли токен
    /// </summary>
    public async Task<bool> IsTokenExpiredAsync()
    {
        var token = await _localStorage.GetItemAsync("accessToken");
        
        if (string.IsNullOrEmpty(token))
            return true;
        
        try
        {
            // Парсим JWT токен для получения exp claim
            var payload = token.Split('.')[1];
            
            // Добавляем паддинги
            var padLength = 4 - (payload.Length % 4);
            if (padLength < 4)
            {
                payload += new string('=', padLength);
            }
            
            var jsonBytes = Convert.FromBase64String(payload);
            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
            
            if (json != null && json.TryGetValue("exp", out var expElement))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64()).DateTime;
                
                // Токен истекает через 5 минут или меньше
                return expTime <= DateTime.UtcNow.AddMinutes(5);
            }
            
            return true; // Если нет exp claim — считаем токен невалидным
        }
        catch
        {
            return true; // Если не удалось распарсить — считаем токен невалидным
        }
    }

    /// <summary>
    /// Обновляет токен через refresh token
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        var accessToken = await _localStorage.GetItemAsync("accessToken");
        var refreshToken = await _localStorage.GetItemAsync("refreshToken");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            // Используем анонимный клиент (без AuthorizationMessageHandler)
            var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI.Anonymous");
            var response = await httpClient.PostAsJsonAsync("/api/auth/refresh", new { accessToken, refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Refresh failed: {response.StatusCode}");
                return false;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse != null)
            {
                await _localStorage.SetItemAsync("accessToken", authResponse.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
                Console.WriteLine($"✅ Token refreshed successfully");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Refresh exception: {ex.Message}");
            return false;
        }

        return false;
    }

    /// <summary>
    /// Проверяет и обновляет токен если нужно
    /// </summary>
    public async Task<bool> EnsureValidTokenAsync()
    {
        if (await IsTokenExpiredAsync())
        {
            Console.WriteLine($"⚠️ Token expired, attempting refresh...");
            return await RefreshTokenAsync();
        }
        
        return true; // Токен ещё валиден
    }
}
