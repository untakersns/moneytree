using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;

namespace MoneyTreeAPI.Application.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryResponseDto>>
{
    public string UserId { get; init; } = string.Empty;
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponseDto>>
{
    private readonly MoneyTreeDBContext _db;

    public GetCategoriesQueryHandler(MoneyTreeDBContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Возвращаем личные категории пользователя + системные (где UserId == null)
        var categories = await _db.Categories
            .Where(c => c.UserId == request.UserId || c.UserId == null)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type.ToString(),
            IsSystem = c.UserId == null
        }).ToList();
    }
}