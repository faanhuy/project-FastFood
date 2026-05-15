using CartEntity = SmartShop.Domain.Entities.Cart;

namespace SmartShop.Application.Features.Cart;

internal static class CartMapper
{
    internal static CartDto ToDto(CartEntity cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ItemType = i.ItemType.ToString(),
                ProductId = i.ProductId,
                ComboId = i.ComboId,
                DisplayName = i.DisplayName,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal,
                SizeId = i.SizeId,
                SizeLabel = i.SizeLabel,
                Components = i.Components.Select(c => new CartItemComponentDto
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    SizeId = c.SizeId,
                    SizeLabel = c.SizeLabel,
                    QuantityPerCombo = c.QuantityPerCombo,
                    TotalQuantity = c.TotalQuantity,
                    UnitPriceSnapshot = c.UnitPriceSnapshot
                }).ToList()
            }).ToList(),
            TotalAmount = cart.TotalAmount
        };
    }
}
