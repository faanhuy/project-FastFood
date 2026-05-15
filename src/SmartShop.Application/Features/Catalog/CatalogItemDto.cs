namespace SmartShop.Application.Features.Catalog;

public class CatalogItemDto
{
    public Guid Id { get; init; }
    public string ItemType { get; init; } = string.Empty; // "Product" | "Combo"

    // Shared display fields
    public string Name { get; init; } = string.Empty;         // product.Name hoặc combo.Title
    public string Slug { get; init; } = string.Empty;         // product.Slug (combo = "")
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }

    // Pricing
    public decimal Price { get; init; }                        // giá bán thực
    public decimal? OriginalPrice { get; init; }               // giá gốc trước giảm
    public decimal? DiscountPercent { get; init; }             // % giảm

    // Product-specific
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public bool HasSizes { get; init; }

    // Combo-specific
    public int? ComboItemCount { get; init; }
    public DateTime? StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }

    public DateTime CreatedAt { get; init; }
}

public class GetCatalogResult
{
    public List<CatalogItemDto> Products { get; init; } = [];
    public List<CatalogItemDto> Combos { get; init; } = [];
    public int TotalProducts { get; init; }
    public int TotalCombos { get; init; }
}
