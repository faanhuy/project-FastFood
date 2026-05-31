using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommandHandler(
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review", request.ReviewId);

        if (!request.IsAdmin && review.UserId != request.RequestingUserId)
            throw new UnauthorizedException("error.review_delete_unauthorized", null);

        reviewRepository.Delete(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
