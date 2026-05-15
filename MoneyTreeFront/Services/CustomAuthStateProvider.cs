using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace MoneyTreeFront.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    private readonly IHttpClientFactory _httpClientFactory;

    public CustomAuthStateProvider(LocalStorageService localStorage, IHttpClientFactory httpClientFactory)
    {
        _localStorage = localStorage;
        _httpClientFactory = httpClientFactory;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Получаем токен из localStorage
            var token = await _localStorage.GetItemAsync("accessToken");

            if (string.IsNullOrEmpty(token))
            {
                return _anonymous;
            }

            // Парсим JWT токен для получения claims и проверки срока действия
            var claims = ParseClaimsFromJwt(token);

            // Проверяем, не истёк ли токен
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null)
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).DateTime;
                if (expTime < DateTime.UtcNow)
                {
            // Токен истёк, пробуем обновить через TokenRefreshService
            try
            {
                var tokenRefreshService = new TokenRefreshService(_httpClientFactory, _localStorage);
                var refreshSuccessful = await tokenRefreshService.RefreshTokenAsync();

                if (refreshSuccessful)
                {
                    // Если обновление удалось, получаем новый токен и продолжаем
                    var newToken = await _localStorage.GetItemAsync("accessToken");
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        var newClaims = ParseClaimsFromJwt(newToken);
                        var newIdentity = new ClaimsIdentity(newClaims, "jwt");
                        var newUser = new ClaimsPrincipal(newIdentity);
                        return new AuthenticationState(newUser);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during token refresh: {ex.Message}");
            }

            // Если обновление не удалось, удаляем токены и возвращаем анонимного пользователя
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            Console.WriteLine($"⚠️ Token refresh failed, user will see unauthorized version");
            return _anonymous;
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return _anonymous;
        }
    }

    private List<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        // Добавляем паддинги для корректного декодирования
        var padLength = 4 - (payload.Length % 4);
        if (padLength < 4)
        {
            payload += new string('=', padLength);
        }

        var jsonBytes = Convert.FromBase64String(payload);
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        if (json != null)
        {
            foreach (var kvp in json)
            {
                if (kvp.Value.ValueKind == JsonValueKind.String)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.GetString()!));
                }
            }
        }

        return claims;
    }
}