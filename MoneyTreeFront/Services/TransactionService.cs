using MoneyTreeFront.DTOs;

namespace MoneyTreeFront.Services;

public class TransactionService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TransactionService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<TransactionDto>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        
        var url = "/api/transactions";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var transactions = await httpClient.GetFromJsonAsync<List<TransactionDto>>(url);
        return transactions ?? new List<TransactionDto>();
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        return await httpClient.GetFromJsonAsync<TransactionDto>($"/api/transactions/{id}");
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionDto dto)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var response = await httpClient.PostAsJsonAsync("/api/transactions", dto);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TransactionDto>();
        return created!;
    }

    public async Task UpdateAsync(int id, UpdateTransactionDto dto)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var response = await httpClient.PutAsJsonAsync($"/api/transactions/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        var response = await httpClient.DeleteAsync($"/api/transactions/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<BalanceDto> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        
        var url = "/api/transactions/balance";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var balance = await httpClient.GetFromJsonAsync<BalanceDto>(url);
        return balance!;
    }

    public async Task<List<ExpenseByCategoryDto>> GetExpensesByCategoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var httpClient = _httpClientFactory.CreateClient("MoneyTreeAPI");
        
        var url = "/api/transactions/expenses-by-category";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var expenses = await httpClient.GetFromJsonAsync<List<ExpenseByCategoryDto>>(url);
        return expenses ?? new List<ExpenseByCategoryDto>();
    }
}
