using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Queries;

public class GetProductReviewsQueryHandler(IReviewRepository reviewRepository)
    : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewDto>>
{
    public async Task<PagedResult<ReviewDto>> Handle(
        GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await reviewRepository.GetPagedByProductAsync(
            request.ProductId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new ReviewDto
        {
            Id           = r.Id,
            UserId       = r.UserId,
            UserFullName = r.User is not null ? $"{r.User.FirstName} {r.User.LastName}" : "Ẩn danh",
            ProductId    = r.ProductId,
            Rating       = r.Rating,
            Comment      = r.Comment,
            CreatedAt    = r.CreatedAt
        });

        return new PagedResult<ReviewDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
