using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Transactions.Commands;

public record UpdateTransactionCommand : IRequest<TransactionResponseDto>
{
    public int Id { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? Date { get; init; }
    public string? Type { get; init; }
    public int? CategoryId { get; init; }
    public string? Comment { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionResponseDto>
{
    private readonly MoneyTreeDBContext _db;

    public UpdateTransactionCommandHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponseDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        // Находим транзакцию и проверяем, что она принадлежит пользователю
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction == null)
        {
            throw new InvalidOperationException("Транзакция не найдена");
        }

        if (transaction.UserId != request.UserId)
        {
            throw new InvalidOperationException("Транзакция не принадлежит пользователю");
        }

        Category? category = null;

        // Обновляем только указанные поля
        if (request.Amount.HasValue)
        {
            transaction.Amount = request.Amount.Value;
        }

        if (request.Date.HasValue)
        {
            transaction.Date = request.Date.Value;
        }

        if (request.Type != null)
        {
            if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType))
            {
                throw new InvalidOperationException("Неверный тип транзакции");
            }
            transaction.Type = transactionType;
        }

        if (request.CategoryId.HasValue)
        {
            // Проверяем категорию
            category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (category == null)
            {
                throw new InvalidOperationException("Категория не найдена");
            }

            if (category.UserId != request.UserId)
            {
                throw new InvalidOperationException("Категория не принадлежит пользователю");
            }

            transaction.CategoryId = request.CategoryId.Value;
        }
        else
        {
            // Загружаем категорию для ответа
            category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == transaction.CategoryId, cancellationToken);
        }

        if (request.Comment != null)
        {
            transaction.Comment = request.Comment;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new TransactionResponseDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Type = transaction.Type.ToString(),
            CategoryId = transaction.CategoryId,
            CategoryName = category?.Name,
            Comment = transaction.Comment
        };
    }
}
