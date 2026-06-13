using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

public class LoyaltyAccount : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public decimal TotalPoints { get; private set; }        // Spendable points
    public decimal LifetimePoints { get; private set; }     // Only increases, used for tier calculation
    public int Tier { get; private set; } = 0;             // 0=Bronze, 1=Silver, 2=Gold, 3=Platinum

    // Navigation
    public User? User { get; private set; }

    private readonly List<PointTransaction> _transactions = [];
    public IReadOnlyCollection<PointTransaction> Transactions => _transactions.AsReadOnly();

    private LoyaltyAccount() { }

    public static LoyaltyAccount Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ConflictException("validation.user_id_invalid", null);

        return new LoyaltyAccount
        {
            UserId = userId,
            TotalPoints = 0,
            LifetimePoints = 0,
            Tier = 0
        };
    }

    // Returns the tier (0-3) based on LifetimePoints
    public int CalculateTier()
    {
        return LifetimePoints switch
        {
            >= 20000 => 3,  // Platinum
            >= 5000 => 2,   // Gold
            >= 1000 => 1,   // Silver
            _ => 0          // Bronze
        };
    }

    // Earn points when order is delivered
    public PointTransaction EarnPoints(decimal points, Guid orderId, string note = "")
    {
        if (points < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

        TotalPoints += points;
        LifetimePoints += points;
        Tier = CalculateTier();

        var transaction = PointTransaction.Create(
            this.Id, orderId, points, PointTransactionType.Earn, note);
        _transactions.Add(transaction);
        return transaction;
    }

    // Redeem points at checkout
    public PointTransaction RedeemPoints(decimal points, Guid orderId, string note = "")
    {
        if (points < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

        if (points > TotalPoints)
            throw new ConflictException("error.loyalty_insufficient_points",
                new Dictionary<string, string> { ["available"] = TotalPoints.ToString(), ["required"] = points.ToString() });

        TotalPoints -= points;

        var transaction = PointTransaction.Create(
            this.Id, orderId, points, PointTransactionType.Redeem, note);
        _transactions.Add(transaction);
        return transaction;
    }

    // Expire points (e.g., after 1 year of inactivity)
    public PointTransaction ExpirePoints(decimal points, string note = "")
    {
        if (points > TotalPoints)
            throw new ConflictException("error.loyalty_insufficient_points", null);

        TotalPoints -= points;

        var transaction = PointTransaction.Create(
            this.Id, null, points, PointTransactionType.Expire, note);
        _transactions.Add(transaction);
        return transaction;
    }

    // Reverse points when a delivered order is cancelled/returned/refunded
    public PointTransaction ReversePoints(decimal points, Guid orderId, string note = "")
    {
        if (points < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

        TotalPoints = Math.Max(0, TotalPoints - points);
        LifetimePoints = Math.Max(0, LifetimePoints - points);
        Tier = CalculateTier();

        var transaction = PointTransaction.Create(
            this.Id, orderId, points, PointTransactionType.Reverse, note);
        _transactions.Add(transaction);
        return transaction;
    }

    // Admin manual adjustment
    public PointTransaction AdjustPoints(decimal pointsDelta, string reason)
    {
        TotalPoints += pointsDelta;
        LifetimePoints = Math.Max(0, LifetimePoints + pointsDelta);
        Tier = CalculateTier();

        var transaction = PointTransaction.Create(
            this.Id, null, Math.Abs(pointsDelta),
            PointTransactionType.Adjust,
            $"Admin: {reason}");
        _transactions.Add(transaction);
        return transaction;
    }
}
