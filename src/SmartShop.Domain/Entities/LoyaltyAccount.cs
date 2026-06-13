using SmartShop.Domain.Common;

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
            throw new ArgumentException("UserId không được để trống.", nameof(userId));

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
    public void EarnPoints(decimal points, Guid orderId, string note = "")
    {
        if (points < 0)
            throw new ArgumentException("Điểm không được âm.", nameof(points));

        TotalPoints += points;
        LifetimePoints += points;
        Tier = CalculateTier();

        var transaction = PointTransaction.Create(
            this.Id, orderId, points, PointTransactionType.Earn, note);
        _transactions.Add(transaction);
    }

    // Redeem points at checkout
    public void RedeemPoints(decimal points, Guid orderId, string note = "")
    {
        if (points < 0)
            throw new ArgumentException("Điểm không được âm.", nameof(points));

        if (points > TotalPoints)
            throw new InvalidOperationException(
                $"Số điểm không đủ. Có: {TotalPoints}, yêu cầu: {points}");

        TotalPoints -= points;
        // LifetimePoints không thay đổi khi redeem

        var transaction = PointTransaction.Create(
            this.Id, orderId, points, PointTransactionType.Redeem, note);
        _transactions.Add(transaction);
    }

    // Expire points (e.g., after 1 year of inactivity)
    public void ExpirePoints(decimal points, string note = "")
    {
        if (points > TotalPoints)
            throw new InvalidOperationException("Không thể xóa điểm hơn số hiện có.");

        TotalPoints -= points;

        var transaction = PointTransaction.Create(
            this.Id, null, points, PointTransactionType.Expire, note);
        _transactions.Add(transaction);
    }

    // Admin manual adjustment
    public void AdjustPoints(decimal pointsDelta, string reason)
    {
        TotalPoints += pointsDelta;
        LifetimePoints = Math.Max(0, LifetimePoints + pointsDelta);
        Tier = CalculateTier();

        var transaction = PointTransaction.Create(
            this.Id, null, Math.Abs(pointsDelta),
            pointsDelta > 0 ? PointTransactionType.Adjust : PointTransactionType.Adjust,
            $"Admin: {reason}");
        _transactions.Add(transaction);
    }
}
