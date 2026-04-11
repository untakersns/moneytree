using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Transactions.Queries;

public record GetIncomeByCategoryQuery : IRequest<List<CategoryAmountDto>>
{
    public string UserId { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetIncomeByCategoryQueryHandler : IRequestHandler<GetIncomeByCategoryQuery, List<CategoryAmountDto>>
{
    private readonly MoneyTreeDBContext _db;

    public GetIncomeByCategoryQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryAmountDto>> Handle(GetIncomeByCategoryQuery request, CancellationToken cancellationToken)
    {
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

        var income = await query
            .Where(t => t.Type == TransactionType.Income)
            .Include(t => t.Category)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new CategoryAmountDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name ?? "Unknown",
                TotalAmount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(e => e.TotalAmount)
            .ToListAsync(cancellationToken);

        return income;
    }
}
