using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Orders;

internal static class OrderMapper
{
    internal static OrderItemDto ToDto(OrderItem i)
    {
        return new OrderItemDto
        {
            ItemType = i.ItemType.ToString(),
            ProductId = i.ProductId,
            ComboId = i.ComboId,
            ProductName = i.ProductName,
            ProductImageUrl = i.ItemType == CartItemType.Product ? i.Product?.ImageUrl ?? i.ImageUrl : i.ImageUrl,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            SubTotal = i.SubTotal,
            SizeId = i.SizeId,
            SizeLabel = i.SizeLabel,
            OriginalUnitPrice = i.OriginalUnitPrice,
            Components = i.Components.Select(c => new OrderItemComponentDto
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                ProductImageUrl = c.ProductImageUrl,
                SizeId = c.SizeId,
                SizeLabel = c.SizeLabel,
                QuantityPerCombo = c.QuantityPerCombo,
                TotalQuantity = c.TotalQuantity,
                UnitPriceSnapshot = c.UnitPriceSnapshot
            }).ToList()
        };
    }
}
