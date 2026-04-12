using MediatR;

namespace SmartShop.Application.Features.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(string Code) : IRequest;
