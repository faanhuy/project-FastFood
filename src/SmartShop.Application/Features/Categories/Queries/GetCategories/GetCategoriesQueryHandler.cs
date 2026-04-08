using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ICacheService cache
) : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private const string CacheKey = "categories:all";

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<List<CategoryDto>>(CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var categories = await categoryRepository.GetAllActiveAsync(cancellationToken);
        var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl)).ToList();

        await cache.SetAsync(CacheKey, dtos, TimeSpan.FromHours(1), cancellationToken);

        return dtos;
    }
}
