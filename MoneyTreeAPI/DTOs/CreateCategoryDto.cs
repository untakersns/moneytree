namespace MoneyTreeAPI.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Income" или "Expense"
}
