using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly ICouponRepository repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICacheService cache;

    public DeleteCouponCommandHandler(ICouponRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.cache = cache;
    }

    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await repository.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException(nameof(Coupon), request.Code);

        var hasUsage = await repository.HasAnyUsageAsync(coupon.Id, cancellationToken);
        if (hasUsage)
            throw new ConflictException($"Coupon '{request.Code}' đã được sử dụng, không thể xoá.");

        repository.DeleteAsync(coupon);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync($"coupons:id:{request.Code}", cancellationToken);
        await cache.RemoveByPrefixAsync("coupons:list:", cancellationToken);
    }
}