using MediatR;
using SmartShop.Application.Common.Exceptions;
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
            throw new UnauthorizedException("Bạn không có quyền xóa đánh giá này.");

        reviewRepository.Delete(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
