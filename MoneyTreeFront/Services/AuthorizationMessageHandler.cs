namespace MoneyTreeFront.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly LocalStorageService _localStorage;

    public AuthorizationMessageHandler(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
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

        return await base.SendAsync(request, cancellationToken);
    }
}
