using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class WishlistItem : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    private WishlistItem() { }

    public static WishlistItem Create(Guid userId, Guid productId)
    {
        return new WishlistItem
        {
            UserId = userId,
            ProductId = productId
        };
    }
}
