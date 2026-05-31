using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Queries;

public record GetProductReviewsQuery(Guid ProductId, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<ReviewDto>>;
