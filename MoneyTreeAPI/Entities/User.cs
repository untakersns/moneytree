using Microsoft.AspNetCore.Identity;

namespace MoneyTreeAPI.Models;

public enum Currency
{
    RUB,
    USD,
    EUR,
    BYN
}

public class User : IdentityUser
{
    public Currency Currency { get; set; } = Currency.RUB;

    // Refresh token
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}