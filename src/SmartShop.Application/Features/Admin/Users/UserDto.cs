namespace SmartShop.Application.Features.Admin.Users;

public class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsBanned { get; init; }
    public DateTime? BannedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public int OrderCount { get; init; }
}
