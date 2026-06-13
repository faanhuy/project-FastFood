using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public enum PointTransactionType
{
    Earn = 1,      // Points earned from order delivery
    Redeem = 2,    // Points used at checkout
    Expire = 3,    // Points expired (inactivity)
    Adjust = 4     // Admin adjustment
}

public class PointTransaction : BaseAuditableEntity
{
    public Guid AccountId { get; private set; }
    public Guid? OrderId { get; private set; }           // Nullable for expires/adjustments
    public decimal Points { get; private set; }
    public PointTransactionType Type { get; private set; }
    public string? Note { get; private set; }

    // Navigation
    public LoyaltyAccount? Account { get; private set; }

    private PointTransaction() { }

    public static PointTransaction Create(
        Guid accountId,
        Guid? orderId,
        decimal points,
        PointTransactionType type,
        string? note = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId không được để trống.", nameof(accountId));
        if (points < 0)
            throw new ArgumentException("Điểm không được âm.", nameof(points));

        return new PointTransaction
        {
            AccountId = accountId,
            OrderId = orderId,
            Points = points,
            Type = type,
            Note = note
        };
    }
}
