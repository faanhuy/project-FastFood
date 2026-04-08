using MediatR;
using SmartShop.Application.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.PlaceOrder;

public class PlaceOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        if (!cart.Items.Any())
            throw new ConflictException("Giỏ hàng đang trống.");

        // Validate stock for all items
        foreach (var item in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException("Product", item.ProductId);

            if (!product.IsActive)
                throw new ConflictException($"Sản phẩm '{product.Name}' không còn bán.");

            if (item.Quantity > product.Stock)
                throw new ConflictException($"Sản phẩm '{product.Name}' chỉ còn {product.Stock} trong kho.");
        }

        // Create order
        var order = Order.Create(request.UserId, request.ShippingAddress, request.Notes);

        foreach (var item in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            var orderItem = OrderItem.Create(order.Id, item.ProductId, product!.Name, item.Quantity, item.UnitPrice);
            order.AddItem(orderItem);

            // Reduce stock
            product.ReduceStock(item.Quantity);
            productRepository.Update(product);
        }

        await orderRepository.AddAsync(order, cancellationToken);

        // Clear cart after successful order
        cart.Clear();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
