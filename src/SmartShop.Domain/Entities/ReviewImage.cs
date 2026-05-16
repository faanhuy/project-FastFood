using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class ReviewImage : BaseAuditableEntity
{
    public Guid ReviewId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public Review? Review { get; private set; }

    private ReviewImage() { }

    public static ReviewImage Create(Guid reviewId, string imageUrl, int displayOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);
        ArgumentOutOfRangeException.ThrowIfNegative(displayOrder);

        return new ReviewImage
        {
            ReviewId = reviewId,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder
        };
    }
}
