namespace MoneyTreeAPI.DTOs;

public class UpdateTransactionDto
{
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Type { get; set; }
    public int? CategoryId { get; set; }
    public string? Comment { get; set; }
}