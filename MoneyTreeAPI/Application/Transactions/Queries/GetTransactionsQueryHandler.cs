using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;

namespace MoneyTreeAPI.Application.Transactions.Queries;

public record GetTransactionsQuery : IRequest<List<TransactionResponseDto>>
{
    public string UserId { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, List<TransactionResponseDto>>
{
    private readonly MoneyTreeDBContext _db;

    public GetTransactionsQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<List<TransactionResponseDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        // Фильтруем транзакции по пользователю и периоду
        var query = _db.Transactions
            .Where(t => t.UserId == request.UserId);

        if (request.StartDate.HasValue)
        {
            var startDate = request.StartDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
                : request.StartDate.Value.ToUniversalTime();
            query = query.Where(t => t.Date >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
                : request.EndDate.Value.ToUniversalTime();
            query = query.Where(t => t.Date <= endDate);
        }

        // Загружаем транзакции с категориями
        var transactions = await query
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

        return transactions.Select(t => new TransactionResponseDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Date = t.Date,
            Type = t.Type.ToString(),
            CategoryId = t.CategoryId,
            CategoryName = t.Category != null ? t.Category.Name : "Unknown",
            Comment = t.Comment
        }).ToList();
    }
}