using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Queries;

public class GetProductReviewsQueryHandler(
    IReviewRepository reviewRepository,
    IReviewImageRepository reviewImageRepository
) : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewDto>>
{
    public async Task<PagedResult<ReviewDto>> Handle(
        GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await reviewRepository.GetPagedByProductAsync(
            request.ProductId, request.Page, request.PageSize, cancellationToken);

        var dtos = new List<ReviewDto>();
        foreach (var r in items)
        {
            var images = await reviewImageRepository.GetByReviewIdAsync(r.Id, cancellationToken);
            dtos.Add(new ReviewDto
            {
                Id           = r.Id,
                UserId       = r.UserId,
                UserFullName = r.UserFullName,
                ProductId    = r.ProductId,
                Rating       = r.Rating,
                Comment      = r.Comment,
                CreatedAt    = r.CreatedAt,
                ImageUrls    = images.Select(img => img.ImageUrl).ToList()
            });
        }

        return new PagedResult<ReviewDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
