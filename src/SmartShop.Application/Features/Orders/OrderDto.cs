namespace SmartShop.Application.Features.Orders;

public class OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal SubTotal { get; init; }
}

public class OrderDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
