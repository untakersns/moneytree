namespace MoneyTreeAPI.DTOs;

public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
    public string Type { get; set; } = string.Empty; // "Income" или "Expense"
    public int CategoryId { get; set; }
    public string? Comment { get; set; }
}