using MediatR;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;

namespace MoneyTreeAPI.Application.Categories.Commands;

public record CreateCategoryCommand : IRequest<CategoryResponseDto>
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    private readonly MoneyTreeDBContext _db;

    public CreateCategoryCommandHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Преобразуем тип категории
        if (!Enum.TryParse<CategoryType>(request.Type, out var categoryType))
        {
            throw new InvalidOperationException("Неверный тип категории");
        }

        var category = new Category
        {
            Name = request.Name,
            Type = categoryType,
            UserId = request.UserId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type.ToString(),
            IsSystem = false
        };
    }
}
