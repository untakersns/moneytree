using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace MoneyTreeFront.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    public CustomAuthStateProvider(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
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
                    // Токен истёк, пробуем обновить
                    await _localStorage.RemoveItemAsync("accessToken");
                    await _localStorage.RemoveItemAsync("refreshToken");
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
