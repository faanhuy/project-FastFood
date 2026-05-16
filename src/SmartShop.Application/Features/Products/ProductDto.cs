namespace SmartShop.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal OriginalPrice,
    string Slug,
    string? ImageUrl,
    bool IsActive,
    Guid CategoryId,
    DateTime CreatedAt,
    bool HasSizes = false,
    string? SizeType = null,
    decimal? EffectivePrice = null
);

public record SizeDto(
    Guid Id,
    string Label,
    int DisplayOrder,
    bool IsActive,
    decimal? EffectivePrice = null
);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal OriginalPrice,
    string Slug,
    string? ImageUrl,
    bool IsActive,
    Guid CategoryId,
    DateTime CreatedAt,
    bool HasSizes,
    string? SizeType,
    IReadOnlyList<SizeDto> Sizes,
    decimal EffectivePrice
);
