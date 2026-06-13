using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using CartEntity = SmartShop.Domain.Entities.Cart;

namespace SmartShop.Application.Features.Cart.Commands.AddFromOrder;

public class AddFromOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddFromOrderCommand, CartDto>
{
    public async Task<CartDto> Handle(AddFromOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, ct)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.UserId != request.UserId)
            throw new UnauthorizedException("error.unauthorized");

        var cart = await cartRepository.GetByUserIdAsync(request.UserId, ct);
        if (cart == null)
        {
            cart = CartEntity.Create(request.UserId);
            await cartRepository.AddAsync(cart, ct);
        }

        foreach (var item in order.Items.Where(i => i.ItemType == CartItemType.Product && i.ProductId.HasValue))
        {
            var product = await productRepository.GetByIdAsync(item.ProductId!.Value, ct);
            if (product == null || !product.IsActive) continue;

            var isNewItem = !cart.Items.Any(ci =>
                ci.ItemType == CartItemType.Product &&
                ci.ProductId == item.ProductId &&
                ci.SizeId == item.SizeId);

            cart.AddItem(product.Id, product.Name, product.ImageUrl,
                item.Quantity, product.Price, item.SizeId, item.SizeLabel);

            if (isNewItem)
            {
                var addedItem = cart.Items.First(ci =>
                    ci.ItemType == CartItemType.Product &&
                    ci.ProductId == item.ProductId &&
                    ci.SizeId == item.SizeId);
                await cartRepository.AddCartItemAsync(addedItem, ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        var updated = await cartRepository.GetByUserIdAsync(request.UserId, ct);
        return CartMapper.ToDto(updated!);
    }
}
