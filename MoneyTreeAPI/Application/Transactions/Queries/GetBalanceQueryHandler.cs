using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Transactions.Queries;

public record GetBalanceQuery : IRequest<BalanceResponseDto>
{
    public string UserId { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, BalanceResponseDto>
{
    private readonly MoneyTreeDBContext _db;

    public GetBalanceQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<BalanceResponseDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
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

        // Считаем суммы
        var totalIncome = await query
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount, cancellationToken);

        var totalExpense = await query
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount, cancellationToken);

        // Получаем валюту пользователя
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        return new BalanceResponseDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            Currency = user?.Currency.ToString() ?? "RUB"
        };
    }
}