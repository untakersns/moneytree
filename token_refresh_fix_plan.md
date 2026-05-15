# План исправления проблем с обновлением токенов

## [Обзор]
Исправление проблем с истечением access token и улучшение обработки ошибок в LocalStorageService.

Текущая реализация имеет несколько проблем:
1. При истечении access token он сразу удаляется без попытки обновления
2. Обработка исключений в RemoveItemAsync недостаточна
3. Нет проверки срока действия refresh token перед попыткой обновления

## [Изменения в CustomAuthStateProvider]

### Текущая реализация (проблемная):
```csharp
// Токен истёк, пробуем обновить
await _localStorage.RemoveItemAsync("accessToken");
await _localStorage.RemoveItemAsync("refreshToken");
return _anonymous;
```

### Новая реализация:
```csharp
// Токен истёк, пробуем обновить через TokenRefreshService
var tokenRefreshService = new TokenRefreshService(httpClientFactory, _localStorage);
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

// Если обновление не удалось, удаляем токены и возвращаем анонимного пользователя
await _localStorage.RemoveItemAsync("accessToken");
await _localStorage.RemoveItemAsync("refreshToken");
return _anonymous;
```

## [Изменения в LocalStorageService]

### Улучшенный RemoveItemAsync:
```csharp
public async ValueTask RemoveItemAsync(string key)
{
    _cache.Remove(key);
    try
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
    catch (InvalidOperationException ex)
    {
        // Игнорируем ошибки во время prerendering
        Console.WriteLine($"LocalStorage remove ignored during prerendering: {ex.Message}");
    }
    catch (JSException ex)
    {
        // Обрабатываем ошибки JSInterop
        Console.WriteLine($"JSInterop error in RemoveItemAsync: {ex.Message}");
    }
    catch (Exception ex)
    {
        // Обрабатываем все остальные ошибки
        Console.WriteLine($"Unexpected error in RemoveItemAsync: {ex.Message}");
    }
}
```

### Новый метод для синхронизации кэша:
```csharp
public async ValueTask SyncCacheAsync()
{
    try
    {
        // Очищаем текущий кэш
        _cache.Clear();

        // Загружаем все ключи из localStorage
        var keys = await _js.InvokeAsync<string[]>("Object.keys", "localStorage");

        if (keys != null)
        {
            foreach (var key in keys)
            {
                var value = await _js.InvokeAsync<string>("localStorage.getItem", key);
                if (value != null)
                {
                    _cache[key] = value;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error syncing cache: {ex.Message}");
    }
}
```

## [Изменения в TokenRefreshService]

### Добавление проверки срока действия refresh token:
```csharp
public async Task<bool> IsRefreshTokenValidAsync()
{
    var refreshToken = await _localStorage.GetItemAsync("refreshToken");

    if (string.IsNullOrEmpty(refreshToken))
        return false;

    try
    {
        // Парсим JWT токен для получения exp claim
        var payload = refreshToken.Split('.')[1];

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
            return expTime > DateTime.UtcNow;
        }

        return false;
    }
    catch
    {
        return false;
    }
}
```

### Модифицированный RefreshTokenAsync:
```csharp
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

    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
    {
        Console.WriteLine($"❌ Refresh failed: tokens are null");
        return false;
    }

    try
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI.Anonymous");
        Console.WriteLine($"🔄 Attempting token refresh...");

        var response = await httpClient.PostAsJsonAsync("/api/auth/refresh", new { accessToken, refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Refresh failed with status {response.StatusCode}: {error}");

            // Если 401, значит refresh token тоже истек
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await _localStorage.RemoveItemAsync("accessToken");
                await _localStorage.RemoveItemAsync("refreshToken");
            }

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

        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Refresh exception: {ex.Message}");
        return false;
    }
}
```

## [Порядок внедрения изменений]

1. **Добавить зависимость TokenRefreshService в CustomAuthStateProvider**
2. **Реализовать улучшенный RemoveItemAsync в LocalStorageService**
3. **Добавить метод синхронизации кэша в LocalStorageService**
4. **Добавить проверку refresh token в TokenRefreshService**
5. **Обновить логику обновления токена в TokenRefreshService**
6. **Протестировать все сценарии**

## [Ожидаемый результат]
После этих изменений:
- Access token будет автоматически обновляться при истечении, если refresh token валиден
- Улучшится обработка ошибок в LocalStorageService
- Будет лучше логирование для отладки
- Пользователь не будет разлогиниваться при истечении access token