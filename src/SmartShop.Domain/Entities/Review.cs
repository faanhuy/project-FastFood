using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class Review : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; }

    public User? User { get; private set; }
    public Product? Product { get; private set; }

    private readonly List<ReviewImage> _images = [];
    public IReadOnlyCollection<ReviewImage> Images => _images.AsReadOnly();

    private Review() { }

    public static Review Create(Guid userId, Guid productId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ConflictException("validation.review_rating_range", null);

        return new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = rating,
            Comment = comment
        };
    }

    public void Approve()
    {
        IsApproved = true;
    }

    public void Update(int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ConflictException("validation.review_rating_range", null);

        Rating = rating;
        Comment = comment;
    }
}
