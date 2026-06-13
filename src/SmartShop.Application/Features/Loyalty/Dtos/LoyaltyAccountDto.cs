namespace SmartShop.Application.Features.Loyalty.Dtos;

public class LoyaltyAccountDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal TotalPoints { get; init; }
    public decimal LifetimePoints { get; init; }
    public int Tier { get; init; }
    public string TierName { get; init; } = string.Empty;  // Bronze, Silver, Gold, Platinum
    public int NextTier { get; init; }                     // -1 if Platinum
    public decimal NextTierPoints { get; init; }           // Points needed for next tier
    public decimal PointsValueVnd { get; init; }           // TotalPoints * 10
    public DateTime CreatedAt { get; init; }
}
