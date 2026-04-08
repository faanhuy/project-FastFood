using MediatR;

namespace SmartShop.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
