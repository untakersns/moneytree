using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTreeAPI.Application.Categories.Commands;
using MoneyTreeAPI.Application.Transactions.Commands;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Services;

namespace MoneyTreeAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public TransactionsController(
        IMediator mediator,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Создать новую транзакцию
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var command = new CreateTransactionCommand
        {
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            Comment = dto.Comment,
            UserId = userId!
        };

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Обновить транзакцию
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var command = new UpdateTransactionCommand
        {
            Id = id,  // ← Берём из URL
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            Comment = dto.Comment,
            UserId = userId!
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Удалить транзакцию
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = _currentUserService.UserId;

        var command = new DeleteTransactionCommand
        {
            Id = id,
            UserId = userId!
        };

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Получить транзакцию по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        // TODO: Добавить query для получения транзакции
        return StatusCode(501, new { message = "Метод будет реализован позже" });
    }

    /// <summary>
    /// Создать новую категорию
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var userId = _currentUserService.UserId;

        var command = new CreateCategoryCommand
        {
            Name = dto.Name,
            Type = dto.Type,
            UserId = userId!
        };

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetCategories), new { id = result.Id }, result);
    }

    /// <summary>
    /// Получить список категорий
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        // TODO: Добавить query для получения категорий
        return StatusCode(501, new { message = "Метод будет реализован позже" });
    }
}
