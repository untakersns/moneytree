using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;

namespace MoneyTreeAPI.Application.Categories.Commands;

public record DeleteCategoryCommand : IRequest
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly MoneyTreeDBContext _db;

    public DeleteCategoryCommandHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        // Находим категорию
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new InvalidOperationException("Категория не найдена");
        }

        // Проверяем, что категория принадлежит пользователю
        if (category.UserId != request.UserId)
        {
            throw new InvalidOperationException("Категория не принадлежит пользователю");
        }


        // Проверяем, что нет транзакций с этой категорией
        var hasTransactions = await _db.Transactions
            .AnyAsync(t => t.CategoryId == request.Id, cancellationToken);

        if (hasTransactions)
        {
            throw new InvalidOperationException("Нельзя удалить категорию, которая используется в транзакциях");
        }

        // Удаляем категорию
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
    }
}