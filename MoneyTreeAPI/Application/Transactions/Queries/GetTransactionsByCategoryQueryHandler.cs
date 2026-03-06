using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Transactions.Queries;
public record GetTransactionsByCategoryQuery : IRequest<List<TransactionByCategoryDto>>
{
    public string UserId { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
public class GetTransactionsByCategoryQueryHandler : IRequestHandler<GetTransactionsByCategoryQuery, List<TransactionByCategoryDto>>
{
    private readonly MoneyTreeDBContext _db;

    public GetTransactionsByCategoryQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<List<TransactionByCategoryDto>> Handle(GetTransactionsByCategoryQuery request, CancellationToken cancellationToken)
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

        // Группируем по категориям и считаем суммы
        var expenses = await query
            .Where(t => t.Type == TransactionType.Expense)
            .Include(t => t.Category)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Type })
            .Select(g => new TransactionByCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name ?? "Unknown",
                TotalAmount = g.Sum(t => t.Amount),
                Type = g.Key.Type.ToString()
            })
            .OrderByDescending(e => e.TotalAmount)
            .ToListAsync(cancellationToken);

        return expenses;
    }
}
