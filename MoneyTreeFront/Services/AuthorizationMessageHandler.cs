namespace MoneyTreeFront.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly LocalStorageService _localStorage;
    private readonly TokenRefreshService _tokenRefreshService;

    public AuthorizationMessageHandler(
        LocalStorageService localStorage,
        TokenRefreshService tokenRefreshService)
    {
        _localStorage = localStorage;
        _tokenRefreshService = tokenRefreshService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Проверяем и обновляем токен если нужно
        var tokenValid = await _tokenRefreshService.EnsureValidTokenAsync();
        
        // Получаем токен из localStorage
        string? token = null;
        try
        {
            token = await _localStorage.GetItemAsync("accessToken");

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"🔑 Токен: {token.Substring(0, 20)}...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка токена: {ex.Message}");
        }

        // Добавляем в заголовок Authorization
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine($"✅ Заголовок добавлен: Bearer {token.Substring(0, 20)}...");
        }
        else
        {
            Console.WriteLine($"❌ Токен пустой, заголовок НЕ добавлен");
        }

        Console.WriteLine($"📤 Запрос: {request.Method} {request.RequestUri}");
        Console.WriteLine($"📋 Заголовки: {request.Headers.Authorization}");

        var response = await base.SendAsync(request, cancellationToken);
        
        // Если получили 401 — пробуем обновить токен и повторить запрос
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"⚠️ Получен 401, пробуем обновить токен...");
            
            var refreshed = await _tokenRefreshService.RefreshTokenAsync();
            
            if (refreshed)
            {
                Console.WriteLine($"✅ Токен обновлён, повторяем запрос...");
                
                // Получаем новый токен
                var newToken = await _localStorage.GetItemAsync("accessToken");
                
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    Console.WriteLine($"✅ Заголовок обновлён: Bearer {newToken.Substring(0, 20)}...");
                    
                    // Повторяем запрос
                    return await base.SendAsync(request, cancellationToken);
                }
            }
            else
            {
                Console.WriteLine($"❌ Не удалось обновить токен");
            }
        }
        
        return response;
    }
}
