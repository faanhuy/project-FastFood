using System.Security.Claims;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.WebAPI.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedException("error.unauthorized", null);

    public string? UserEmail =>
        User?.FindFirstValue(ClaimTypes.Email);

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.GivenName) is string first
        && User?.FindFirstValue(ClaimTypes.Surname) is string last
            ? $"{first} {last}".Trim()
            : User?.FindFirstValue(ClaimTypes.Name);
}
