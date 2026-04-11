using MediatR;
using SmartShop.Application.Features.Reviews;

namespace SmartShop.Application.Features.Reviews.Commands.AddReview;

public record AddReviewCommand(Guid UserId, Guid ProductId, int Rating, string Comment)
    : IRequest<ReviewDto>;
