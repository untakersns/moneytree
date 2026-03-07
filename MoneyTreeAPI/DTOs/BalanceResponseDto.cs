namespace MoneyTreeAPI.DTOs;

public class BalanceResponseDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}