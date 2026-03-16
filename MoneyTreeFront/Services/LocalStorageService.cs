using Microsoft.JSInterop;

namespace MoneyTreeFront.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (InvalidOperationException)
        {
            // JSInterop недоступен во время prerendering
            return null;
        }
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (InvalidOperationException)
        {
            // Игнорируем ошибки во время prerendering
        }
    }

    public async ValueTask ClearAsync()
    {
        await _js.InvokeVoidAsync("localStorage.clear");
    }
}
