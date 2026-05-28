using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Transactions.Commands;

public record CreateTransactionCommand : IRequest<TransactionResponseDto>
{
    public decimal Amount { get; init; }
    public DateTime? Date { get; init; }
    public string Type { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string? Comment { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponseDto>
{
    private readonly MoneyTreeDBContext _db;

    public CreateTransactionCommandHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponseDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, что категория существует и принадлежит пользователю или системная
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new InvalidOperationException("Категория не найдена");
        }

        // Категория должна принадлежать пользователю или быть системной (UserId == null)
        if (category.UserId != null && category.UserId != request.UserId)
        {
            throw new InvalidOperationException("Категория не принадлежит пользователю");
        }

        // Преобразуем тип транзакции
        if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType))
        {
            throw new InvalidOperationException("Неверный тип транзакции");
        }

        var transaction = new Models.Transaction
        {
            Amount = request.Amount,
            Date = request.Date ?? DateTime.UtcNow,  // ← По умолчанию текущая дата
            Type = transactionType,
            CategoryId = request.CategoryId,
            UserId = request.UserId,
            Comment = request.Comment
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        return new TransactionResponseDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Type = transaction.Type.ToString(),
            CategoryId = transaction.CategoryId,
            CategoryName = category.Name,
            Comment = transaction.Comment
        };
    }
}