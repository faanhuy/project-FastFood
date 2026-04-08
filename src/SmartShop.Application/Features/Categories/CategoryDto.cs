namespace SmartShop.Application.Features.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl
);
