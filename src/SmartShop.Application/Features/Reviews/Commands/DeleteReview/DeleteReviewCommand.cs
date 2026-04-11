using MediatR;

namespace SmartShop.Application.Features.Reviews.Commands.DeleteReview;

public record DeleteReviewCommand(Guid ReviewId, Guid RequestingUserId, bool IsAdmin) : IRequest;
