using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    ICouponUsageRepository couponUsageRepository,
    IStoreInventoryRepository storeInventoryRepository,
    IStoreSizeInventoryRepository storeSizeInventoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.UserId != request.UserId)
            throw new UnauthorizedException("error.order_cancel_unauthorized", null);

        if (order.Status != OrderStatus.Pending)
            throw new ConflictException("error.order_cancel_invalid_status", null);

        order.Cancel();

        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            var coupon = await couponRepository.GetByCodeAsync(order.CouponCode, cancellationToken);
            if (coupon is not null)
            {
                coupon.Refund();
                couponRepository.Update(coupon);
            }

            var usage = await couponUsageRepository.GetByOrderIdAsync(order.Id, cancellationToken);
            if (usage is not null)
                couponUsageRepository.Delete(usage);
        }

        if (order.StoreId.HasValue)
        {
            foreach (var item in order.Items)
            {
                if (item.ItemType == CartItemType.Product)
                {
                    await RestoreProductItemStockAsync(order.StoreId.Value, item, cancellationToken);
                }
                else if (item.ItemType == CartItemType.Combo)
                {
                    await RestoreComboItemStockAsync(order.StoreId.Value, item, cancellationToken);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task RestoreProductItemStockAsync(
        Guid storeId, Domain.Entities.OrderItem item, CancellationToken ct)
    {
        if (item.SizeId.HasValue)
        {
            var sizeInv = await storeSizeInventoryRepository.GetAsync(
                storeId, item.ProductId!.Value, item.SizeId.Value, ct);

            if (sizeInv is not null)
            {
                sizeInv.RestoreStock(item.Quantity);
                storeSizeInventoryRepository.Update(sizeInv);
            }

            await RestoreStoreInventoryAsync(storeId, item.ProductId!.Value, item.Quantity, ct);
        }
        else
        {
            await RestoreStoreInventoryAsync(storeId, item.ProductId!.Value, item.Quantity, ct);
        }
    }

    private async Task RestoreComboItemStockAsync(
        Guid storeId, Domain.Entities.OrderItem item, CancellationToken ct)
    {
        foreach (var c in item.Components)
        {
            if (c.SizeId.HasValue)
            {
                var sizeInv = await storeSizeInventoryRepository.GetAsync(
                    storeId, c.ProductId, c.SizeId.Value, ct);

                if (sizeInv is not null)
                {
                    sizeInv.RestoreStock(c.TotalQuantity);
                    storeSizeInventoryRepository.Update(sizeInv);
                }

                await RestoreStoreInventoryAsync(storeId, c.ProductId, c.TotalQuantity, ct);
            }
            else
            {
                await RestoreStoreInventoryAsync(storeId, c.ProductId, c.TotalQuantity, ct);
            }
        }
    }

    private async Task RestoreStoreInventoryAsync(
        Guid storeId, Guid productId, int quantity, CancellationToken ct)
    {
        var inventory = await storeInventoryRepository.GetByStoreAndProductAsync(storeId, productId, ct);

        if (inventory is null)
        {
            inventory = Domain.Entities.StoreInventory.Create(storeId, productId, quantity);
            await storeInventoryRepository.AddAsync(inventory, ct);
            return;
        }

        inventory.RestoreStock(quantity);
        storeInventoryRepository.Update(inventory);
    }
}
