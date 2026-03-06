using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTreeAPI.Application.Categories.Commands;
using MoneyTreeAPI.Application.Categories.Queries;
using MoneyTreeAPI.Application.Transactions.Commands;
using MoneyTreeAPI.Application.Transactions.Queries;
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
    /// Получить список категорий
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var userId = _currentUserService.UserId;

        var query = new GetCategoriesQuery
        {
            UserId = userId!
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Получить баланс
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = _currentUserService.UserId;

        var query = new GetBalanceQuery
        {
            UserId = userId!,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Получить транзакции за период
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = _currentUserService.UserId;

        var query = new GetTransactionsQuery
        {
            UserId = userId!,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Получить расходы по категориям
    /// </summary>
    [HttpGet("transactions-by-category")]
    public async Task<IActionResult> GetTransactionsByCategory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = _currentUserService.UserId;

        var query = new GetTransactionsByCategoryQuery
        {
            UserId = userId!,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);

        return Ok(result);
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
    /// Получить транзакцию по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        var userId = _currentUserService.UserId;

        var query = new GetTransactionByIdQuery
        {
            Id = id,
            UserId = userId!
        };

        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Транзакция не найдена" });
        }

        return Ok(result);
    }
}
