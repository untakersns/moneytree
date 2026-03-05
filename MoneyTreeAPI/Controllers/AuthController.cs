using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyTreeAPI.DTOs;
using MoneyTreeAPI.Models;
using MoneyTreeAPI.Services;

namespace MoneyTreeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Currency = Models.Currency.RUB
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

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email!,
            UserId = user.Id,
            RefreshToken = refreshToken
        });
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
        var userData = _jwtService.GetPrincipalFromExpiredToken(dto.RefreshToken);
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
