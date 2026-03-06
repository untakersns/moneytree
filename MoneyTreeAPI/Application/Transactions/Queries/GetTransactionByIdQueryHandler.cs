using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;

namespace MoneyTreeAPI.Application.Transactions.Queries;
public record GetTransactionByIdQuery : IRequest<TransactionResponseDto?>
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
}
public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionResponseDto?>
{
    private readonly MoneyTreeDBContext _db;

    public GetTransactionByIdQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponseDto?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        // Находим транзакцию по ID и проверяем, что она принадлежит пользователю
        var transaction = await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction == null)
        {
            return null;
        }

        // Проверяем, что транзакция принадлежит текущему пользователю
        if (transaction.UserId != request.UserId)
        {
            throw new InvalidOperationException("Транзакция не принадлежит пользователю");
        }

        return new TransactionResponseDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Type = transaction.Type.ToString(),
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name ?? "Unknown",
            Comment = transaction.Comment
        };
    }
}
