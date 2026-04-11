using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTreeAPI.DBs;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;
using MoneyTreeAPI.Services;
using System.Security.Claims;

namespace MoneyTreeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    private readonly MoneyTreeDBContext _db;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        ICurrentUserService currentUserService,
        MoneyTreeDBContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
        _db = db;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Парсим валюту
        if (!Enum.TryParse<Models.Currency>(dto.Currency, out var currency))
        {
            currency = Models.Currency.RUB;
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Currency = currency
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        // Создаём базовые категории для нового пользователя
        await CreateDefaultCategories(user.Id);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email!,
            UserId = user.Id,
            RefreshToken = refreshToken
        });
    }

    private async Task CreateDefaultCategories(string userId)
    {
        var defaultCategories = new List<Category>
        {
            // Расходы
            new() { Name = "Продукты", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Транспорт", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Кафе и рестораны", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Здоровье", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Одежда", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Развлечения", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Коммунальные услуги", Type = CategoryType.Expense, UserId = userId },
            new() { Name = "Связь и интернет", Type = CategoryType.Expense, UserId = userId },
            // Доходы
            new() { Name = "Зарплата", Type = CategoryType.Income, UserId = userId },
            new() { Name = "Фриланс", Type = CategoryType.Income, UserId = userId },
            new() { Name = "Подработка", Type = CategoryType.Income, UserId = userId },
        };

        _db.Categories.AddRange(defaultCategories);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Вход пользователя (логин)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Неверный email или пароль" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Неверный email или пароль" });
        }

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email!,
            UserId = user.Id,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// Обновление access-токена через refresh-токен
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var userData = _jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (userData == null)
        {
            return Unauthorized(new { message = "Неверный токен" });
        }

        var userId = userData.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Неверный refresh-токен" });
        }

        var newAccessToken = _jwtService.GenerateToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            Email = user.Email!,
            UserId = user.Id,
            RefreshToken = newRefreshToken
        });
    }

    /// <summary>
    /// Выход пользователя (logout) — отзыв refresh-токена
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        // Проверяем refresh-токен
        if (user.RefreshToken != request.RefreshToken)
        {
            return BadRequest(new { message = "Неверный refresh-токен" });
        }

        // Отозываем refresh-токен
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Выход выполнен успешно" });
    }
}