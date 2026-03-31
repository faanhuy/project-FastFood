namespace SmartShop.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal OriginalPrice,
    int Stock,
    string Slug,
    string? ImageUrl,
    bool IsActive,
    Guid CategoryId,
    DateTime CreatedAt
);
