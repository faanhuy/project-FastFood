namespace SmartShop.Application.Features.Combos;

public class ComboItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid? SizeId { get; init; }
    public string? SizeLabel { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPriceSnapshot { get; init; }
    public decimal CurrentUnitPrice { get; init; }
}

public class ComboDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal CurrentOriginalPrice { get; init; }
    public decimal SalePrice { get; init; }
    public bool IsActive { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public bool IsCurrentlyActive { get; init; }
    public List<ComboItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public class ComboSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal CurrentOriginalPrice { get; init; }
    public decimal SalePrice { get; init; }
    public bool IsActive { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public bool IsCurrentlyActive { get; init; }
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
