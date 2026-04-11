namespace MoneyTreeAPI.DTOs;

public class CategoryAmountDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
