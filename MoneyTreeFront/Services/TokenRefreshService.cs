using MoneyTreeFront.DTOs;
using System.Text.Json;

namespace MoneyTreeFront.Services;

// Сервис для проверки и обновления JWT токена
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

    // Проверяет, истёк ли токен
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

    // Проверяет валидность refresh token
    public async Task<bool> IsRefreshTokenValidAsync()
    {
        var refreshToken = await _localStorage.GetItemAsync("refreshToken");

        // Для простой случайной строки мы не можем проверить срок действия на фронтенде
        // Это делается на бэкенде в AuthController.Refresh()
        return !string.IsNullOrEmpty(refreshToken);
    }

    // Обновляет токен через refresh token
    public async Task<bool> RefreshTokenAsync()
    {
        // Проверяем валидность refresh token перед попыткой обновления
        if (!await IsRefreshTokenValidAsync())
        {
            Console.WriteLine("❌ Refresh token is expired or invalid");
            return false;
        }

        var accessToken = await _localStorage.GetItemAsync("accessToken");
        var refreshToken = await _localStorage.GetItemAsync("refreshToken");

        Console.WriteLine($"🔑 Using tokens from localStorage:");
        Console.WriteLine($"   Access: {accessToken?.Substring(0, 20)}...");
        Console.WriteLine($"   Refresh: {refreshToken?.Substring(0, 20)}...");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            Console.WriteLine($"❌ Refresh failed: tokens are null");
            return false;
        }

        try
        {
            // Используем анонимный клиент (без AuthorizationMessageHandler)
            var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI.Anonymous");
            
            Console.WriteLine($"🔄 Attempting token refresh...");
            
            var response = await httpClient.PostAsJsonAsync("/api/auth/refresh", new { accessToken, refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Refresh failed with status {response.StatusCode}: {error}");

                // Если 401, значит refresh token тоже истек - очищаем все токены
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"⚠️ Refresh token is invalid on backend, clearing all tokens");
                    await _localStorage.ClearAsync();
                    return false;
                }

                // При других ошибках (сетевые проблемы и т.д.) не очищаем токены
                // чтобы можно было попробовать снова
                Console.WriteLine($"⚠️ Refresh failed due to {response.StatusCode}, keeping tokens for retry");
                return false;
            }

            Console.WriteLine($"✅ Refresh successful, parsing response...");
            
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse != null)
            {
                await _localStorage.SetItemAsync("accessToken", authResponse.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
                Console.WriteLine($"✅ Tokens saved to localStorage");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Response content is null");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Refresh exception: {ex.Message}");
            return false;
        }
    }

    // Проверяет и обновляет токен если нужно
    public async Task<bool> EnsureValidTokenAsync()
    {
        if (await IsTokenExpiredAsync())
        {
            return await RefreshTokenAsync();
        }

        return true; // Токен ещё валиден
    }
}