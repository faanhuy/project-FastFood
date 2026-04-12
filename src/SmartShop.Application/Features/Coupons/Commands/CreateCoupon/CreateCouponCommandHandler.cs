using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponResponse>
{
    private readonly ICouponRepository repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICacheService cache;

    public CreateCouponCommandHandler(
        ICouponRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.cache = cache;
    }

    public async Task<CouponResponse> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {    
        var existing = await repository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Code '{request.Code}' đã được sử dụng.");

        var coupon = Coupon.Create(
            request.Code, request.DiscountType, request.DiscountValue, request.ExpiresAt, request.MaxUsage, request.Description ?? string.Empty, request.MinOrderValue
        );

        await repository.AddAsync(coupon, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveByPrefixAsync("coupons:list:", cancellationToken);

        return new CouponResponse(
            coupon.Id, coupon.Code, coupon.DiscountType, coupon.DiscountValue, coupon.MinOrderValue, coupon.MaxUsage, coupon.UsedQuantity, coupon.ExpiresAt, coupon.Description);
    }
}