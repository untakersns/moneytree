using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MoneyTreeAPI.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
