using Microsoft.AspNetCore.Components.Authorization;
using MoneyTreeFront.Components;
using MoneyTreeFront.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Регистрация сервисов
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<TokenRefreshService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<CategoryService>();
// HttpClient для авторизованных запросов (с токеном)
builder.Services.AddHttpClient("MoneyTreeAPI", (sp, client) =>
{
    client.BaseAddress = new Uri("https://localhost:7027");
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// HttpClient для анонимных запросов (refresh token)
builder.Services.AddHttpClient("MoneyTreeAPI.Anonymous", (sp, client) =>
{
    client.BaseAddress = new Uri("https://localhost:7027");
});

builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("MoneyTreeAPI");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();