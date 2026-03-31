namespace SmartShop.Application.Features.Auth.Dtos;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiry,
    string Email,
    string FirstName,
    string LastName,
    string Role
);
