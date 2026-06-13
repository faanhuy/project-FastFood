namespace SmartShop.Application.Features.Auth.Dtos;

public class GoogleTokenValidationResult
{
    public string GoogleUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
