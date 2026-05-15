namespace SmartShop.Application.Features.Cart;

public class CartItemComponentDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid? SizeId { get; init; }
    public string? SizeLabel { get; init; }
    public int QuantityPerCombo { get; init; }
    public int TotalQuantity { get; init; }
    public decimal UnitPriceSnapshot { get; init; }
}

public class CartItemDto
{
    public Guid Id { get; init; }
    public string ItemType { get; init; } = "Product";
    public Guid? ProductId { get; init; }
    public Guid? ComboId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal SubTotal { get; init; }
    public Guid? SizeId { get; init; }
    public string? SizeLabel { get; init; }
    public List<CartItemComponentDto> Components { get; init; } = [];
}

public class CartDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public List<CartItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
}
