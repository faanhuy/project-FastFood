using System.Security.Claims;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.WebAPI.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

    public string? UserEmail =>
        User?.FindFirstValue(ClaimTypes.Email);

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.GivenName) is string first
        && User?.FindFirstValue(ClaimTypes.Surname) is string last
            ? $"{first} {last}".Trim()
            : User?.FindFirstValue(ClaimTypes.Name);
}
