using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Commands.AddReview;

public class AddReviewCommandHandler(
    IReviewRepository reviewRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var existing = await reviewRepository.GetByUserAndProductAsync(
            request.UserId, request.ProductId, cancellationToken);

        if (existing is not null)
            throw new ConflictException("error.review_already_exists", null);

        var review = Review.Create(request.UserId, request.ProductId, request.Rating, request.Comment);
        review.Approve(); // tự approve — có thể thay bằng moderation flow sau

        await reviewRepository.AddAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id           = review.Id,
            UserId       = review.UserId,
            UserFullName = string.Empty, // user chưa load trong context này
            ProductId    = review.ProductId,
            Rating       = review.Rating,
            Comment      = review.Comment,
            CreatedAt    = review.CreatedAt
        };
    }
}
