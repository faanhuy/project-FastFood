namespace SmartShop.Application.Features.Loyalty.Dtos;

public class PointTransactionDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public Guid? OrderId { get; init; }
    public decimal Points { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
