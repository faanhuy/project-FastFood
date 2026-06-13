using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Returns.Commands.ApproveReturn;

public class ApproveReturnCommandHandler(
    IReturnRequestRepository returnRequestRepository,
    IOrderRepository orderRepository,
    IStoreInventoryRepository storeInventoryRepository,
    IStoreSizeInventoryRepository storeSizeInventoryRepository,
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    INotificationHubService hubService,
    IMediator mediator,
    IUnitOfWork unitOfWork,
    ILogger<ApproveReturnCommandHandler> logger) : IRequestHandler<ApproveReturnCommand, ReturnRequestDto>
{
    public async Task<ReturnRequestDto> Handle(
        ApproveReturnCommand request,
        CancellationToken cancellationToken)
    {
        // Get return request
        var returnRequest = await returnRequestRepository.GetByIdAsync(request.ReturnRequestId, cancellationToken)
            ?? throw new NotFoundException("Return Request", request.ReturnRequestId);

        // Validate status is Pending
        if (returnRequest.Status != ReturnStatus.Pending)
            throw new ConflictException("error.return_approve_invalid_status", null);

        // Get order with items
        var order = await orderRepository.GetByIdWithItemsAsync(returnRequest.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), returnRequest.OrderId);

        // Approve the return request
        returnRequest.Approve(request.AdminNote);

        // Update order status to Returned
        order.UpdateStatus(OrderStatus.Returned);

        // Restore stock for all items in the order
        await RestoreStock(order, cancellationToken);

        returnRequestRepository.Update(returnRequest);

        var userIdString = returnRequest.UserId.ToString();
        var orderCode = order.Id.ToString()[..8].ToUpper();
        var refundFormatted = returnRequest.RefundAmount.ToString("N0");
        const string titleKey = "notification.returnApprovedTitle";
        const string messageKey = "notification.returnApprovedBody";
        var paramsJson = JsonSerializer.Serialize(new { orderCode, refund = refundFormatted });

        var notification = Notification.Create(returnRequest.UserId, titleKey, messageKey, paramsJson, order.Id);
        await notificationRepository.AddAsync(notification, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event to send email
        var user = await userRepository.GetByIdAsync(returnRequest.UserId, cancellationToken);
        if (user is not null)
        {
            await mediator.Publish(new ReturnApprovedEvent(
                ReturnRequestId: returnRequest.Id,
                OrderId: order.Id,
                UserId: user.Id,
                UserEmail: user.Email,
                UserName: $"{user.FirstName} {user.LastName}".Trim(),
                RefundAmount: returnRequest.RefundAmount,
                AdminNote: returnRequest.AdminNote), cancellationToken);
        }

        try
        {
            await hubService.SendToUserAsync(userIdString, "ReturnRequestUpdated", new
            {
                NotificationId = notification.Id,
                TitleKey = titleKey,
                MessageKey = messageKey,
                Params = paramsJson,
                OrderId = order.Id,
                Status = "Approved",
                RefundAmount = returnRequest.RefundAmount
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Push SignalR notification cho return request {Id} thất bại.", returnRequest.Id);
        }

        return ReturnRequestMapper.ToDto(returnRequest, order.Id.ToString());
    }

    private async Task RestoreStock(
        Order order,
        CancellationToken cancellationToken)
    {
        if (order.StoreId == null)
            throw new ConflictException("error.return_no_store_info", null);

        var storeId = order.StoreId.Value;
        var productItems = order.Items.Where(i => i.ItemType == CartItemType.Product).ToList();
        var comboItems = order.Items.Where(i => i.ItemType == CartItemType.Combo).ToList();

        // Collect all product IDs and size IDs to restore
        var allProductIds = productItems.Select(i => i.ProductId!.Value)
            .Concat(comboItems.SelectMany(ci => ci.Components.Select(c => c.ProductId)))
            .Distinct()
            .ToList();

        var allSizeIds = productItems.Where(i => i.SizeId.HasValue).Select(i => i.SizeId!.Value)
            .Concat(comboItems.SelectMany(ci => ci.Components.Where(c => c.SizeId.HasValue).Select(c => c.SizeId!.Value)))
            .Distinct()
            .ToList();

        // Load inventories
        var inventories = allProductIds.Count > 0
            ? (await storeInventoryRepository.GetByStoreAndProductsAsync(storeId, allProductIds, cancellationToken))
                .ToDictionary(i => i.ProductId)
            : new Dictionary<Guid, StoreInventory>();

        var sizeInventories = allSizeIds.Count > 0
            ? (await storeSizeInventoryRepository.GetByStoreAndSizesAsync(storeId, allSizeIds, cancellationToken))
                .ToDictionary(i => i.SizeId)
            : new Dictionary<Guid, StoreSizeInventory>();

        // Restore product items stock
        foreach (var item in productItems)
        {
            var pid = item.ProductId!.Value;
            if (item.SizeId.HasValue)
            {
                if (sizeInventories.TryGetValue(item.SizeId.Value, out var sizeInv))
                    sizeInv.RestoreStock(item.Quantity);

                if (inventories.TryGetValue(pid, out var inv))
                    inv.RestoreStock(item.Quantity);
            }
            else
            {
                if (inventories.TryGetValue(pid, out var inv))
                    inv.RestoreStock(item.Quantity);
            }
        }

        // Restore combo items stock (restore components)
        foreach (var comboItem in comboItems)
        {
            foreach (var component in comboItem.Components)
            {
                if (component.SizeId.HasValue)
                {
                    if (sizeInventories.TryGetValue(component.SizeId.Value, out var sizeInv))
                        sizeInv.RestoreStock(component.TotalQuantity);

                    if (inventories.TryGetValue(component.ProductId, out var inv))
                        inv.RestoreStock(component.TotalQuantity);
                }
                else
                {
                    if (inventories.TryGetValue(component.ProductId, out var inv))
                        inv.RestoreStock(component.TotalQuantity);
                }
            }
        }
    }
}
