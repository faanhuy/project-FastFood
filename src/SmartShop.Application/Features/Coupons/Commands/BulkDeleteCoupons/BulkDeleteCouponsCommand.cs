using MediatR;
using SmartShop.Application.Features.Common;

namespace SmartShop.Application.Features.Coupons.Commands.BulkDeleteCoupons;

public record BulkDeleteCouponsCommand(
    List<Guid> CouponIds,
    Guid AdminUserId
) : IRequest<BulkActionResult>;
