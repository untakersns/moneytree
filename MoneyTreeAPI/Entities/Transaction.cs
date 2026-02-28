namespace MoneyTreeAPI.Models;

public enum TransactionType
{
    Income,
    Expense
}

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string? Comment { get; set; }
}
