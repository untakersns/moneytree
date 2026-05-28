using MoneyTreeFront.DTOs;

namespace MoneyTreeFront.Services;

public class CategoryService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoryService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var categories = await httpClient.GetFromJsonAsync<List<CategoryDto>>("/api/transactions/categories");
        return categories ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var response = await httpClient.PostAsJsonAsync("/api/transactions/categories", dto);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        return created!;
    }

    public async Task DeleteAsync(int categoryId)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var response = await httpClient.DeleteAsync($"/api/transactions/categories/{categoryId}");
        response.EnsureSuccessStatusCode();
    }
}
