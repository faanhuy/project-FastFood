using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public enum PointTransactionType
{
    Earn = 1,      // Points earned from order delivery
    Redeem = 2,    // Points used at checkout
    Expire = 3,    // Points expired (inactivity)
    Adjust = 4,    // Admin adjustment
    Reverse = 5    // Points reversed when delivered order is cancelled/returned/refunded
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
            throw new ConflictException("validation.user_id_invalid", null);
        if (points < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

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
