using MediatR;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Orders.Commands.PlaceOrder;

public record PlaceOrderCommand(
    Guid UserId,
    Guid StoreId,
    Guid AddressId,
    string? Notes,
    string? CouponCode,
    decimal UsePoints = 0,
    PaymentMethod PaymentMethod = PaymentMethod.COD) : IRequest<OrderDto>;
    