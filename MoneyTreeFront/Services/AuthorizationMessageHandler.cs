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
        var token = await _localStorage.GetItemAsync("accessToken");

        // Добавляем в заголовок Authorization
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
