using MediatR;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Commands.BulkDeleteCoupons;

public class BulkDeleteCouponsCommandHandler(
    ICouponRepository repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BulkDeleteCouponsCommand, BulkActionResult>
{
    public async Task<BulkActionResult> Handle(BulkDeleteCouponsCommand request, CancellationToken cancellationToken)
    {
        // Max 100 items per batch
        if (request.CouponIds.Count > 100)
            throw new ConflictException("error.bulk_max_items", null);

        var coupons = await repository.GetByIdsAsync(request.CouponIds, cancellationToken);
        var errors = new List<BulkItemError>();
        var succeeded = 0;

        foreach (var id in request.CouponIds)
        {
            var coupon = coupons.FirstOrDefault(c => c.Id == id);
            if (coupon is null)
            {
                errors.Add(new BulkItemError(id, "Coupon not found"));
                continue;
            }

            try
            {
                // Check if coupon has any usage
                var hasUsage = await repository.HasAnyUsageAsync(id, cancellationToken);
                if (hasUsage)
                {
                    errors.Add(new BulkItemError(id, "Coupon is in use and cannot be deleted"));
                    continue;
                }

                repository.DeleteAsync(coupon, cancellationToken);
                succeeded++;
            }
            catch (Exception ex)
            {
                errors.Add(new BulkItemError(id, ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(succeeded, errors.Count, errors);
    }
}
