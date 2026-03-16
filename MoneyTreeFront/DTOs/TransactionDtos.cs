namespace MoneyTreeFront.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Comment { get; set; }
}

public class UpdateTransactionDto
{
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Type { get; set; }
    public int? CategoryId { get; set; }
    public string? Comment { get; set; }
}

public class BalanceDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class ExpenseByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Type { get; set; } = string.Empty;
}
