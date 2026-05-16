namespace SmartShop.Application.Features.Reviews;

public class ReviewDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<string> ImageUrls { get; init; } = [];
}
