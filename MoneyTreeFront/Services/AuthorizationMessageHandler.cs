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
        if (!tokenValid)
        {
            Console.WriteLine($"⚠️ Token refresh failed before sending request");
        }

        // Получаем токен из localStorage
        var token = await _localStorage.GetItemAsync("accessToken");
        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"🔑 Using token: {token.Substring(0, 20)}...");
        }
        else
        {
            Console.WriteLine($"⚠️ No token available");
        }

        // Добавляем в заголовок Authorization
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Если получили 401 — пробуем обновить токен и повторить запрос
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"⚠️ Received 401, attempting token refresh...");
            
            var refreshed = await _tokenRefreshService.RefreshTokenAsync();

            if (refreshed)
            {
                Console.WriteLine($"✅ Token refreshed, retrying request...");
                
                // Получаем новый токен
                var newToken = await _localStorage.GetItemAsync("accessToken");

                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);

                    // Повторяем запрос
                    return await base.SendAsync(request, cancellationToken);
                }
            }
            else
            {
                Console.WriteLine($"❌ Token refresh failed after 401");
            }
        }

        return response;
    }
}