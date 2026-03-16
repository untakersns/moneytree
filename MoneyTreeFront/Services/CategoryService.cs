using MoneyTreeFront.DTOs;

namespace MoneyTreeFront.Services;

public class CategoryService
{
    private readonly HttpClient _httpClient;

    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("/api/transactions/categories");
        return categories ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/transactions/categories", dto);
        response.EnsureSuccessStatusCode();
        
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        return created!;
    }
}
