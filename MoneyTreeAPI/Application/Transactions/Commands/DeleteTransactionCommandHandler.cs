using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;

namespace MoneyTreeAPI.Application.Transactions.Commands;

public record DeleteTransactionCommand : IRequest<Unit>
{
    public int Id { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly MoneyTreeDBContext _db;

    public DeleteTransactionCommandHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
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

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}