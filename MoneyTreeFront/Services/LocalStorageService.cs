using Microsoft.JSInterop;

namespace MoneyTreeFront.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    private static readonly Dictionary<string, string> _cache = new();

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        // Сохраняем в кэш (доступен всегда)
        _cache[key] = value;

        // Сохраняем в localStorage браузера
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (InvalidOperationException)
        {
            // JSInterop недоступен во время prerendering
        }
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        // Сначала пробуем из кэша
        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        // Если нет — читаем из localStorage
        try
        {
            var value = await _js.InvokeAsync<string?>("localStorage.getItem", key);
            if (value != null)
            {
                _cache[key] = value;
            }
            return value;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

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

    public async ValueTask ClearAsync()
    {
        _cache.Clear();
        try
        {
            await _js.InvokeVoidAsync("localStorage.clear");
        }
        catch (InvalidOperationException ex)
        {
            // Игнорируем ошибки во время prerendering
            Console.WriteLine($"LocalStorage clear ignored during prerendering: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Обрабатываем все остальные ошибки
            Console.WriteLine($"Unexpected error in ClearAsync: {ex.Message}");
        }
    }

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
}