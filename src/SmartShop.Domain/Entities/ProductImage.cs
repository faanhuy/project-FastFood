using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class ProductImage : BaseAuditableEntity
{
    private ProductImage() { }

    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    public static ProductImage Create(Guid productId, string imageUrl, bool isPrimary = false, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);
        return new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary,
            SortOrder = sortOrder
        };
    }

    public void SetAsPrimary() => IsPrimary = true;
    public void UnsetPrimary() => IsPrimary = false;
    public void UpdateSortOrder(int order) => SortOrder = order;
}
