using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Queries.GetOrderTimeline;

public class GetOrderTimelineQueryHandler(
    IOrderRepository orderRepository,
    IOrderStatusHistoryRepository statusHistoryRepository,
    IReturnRequestRepository returnRequestRepository) : IRequestHandler<GetOrderTimelineQuery, List<OrderTimelineEventDto>>
{
    public async Task<List<OrderTimelineEventDto>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
    {
        // 1. Load order
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // 2. Validate authorization
        if (!request.IsAdmin && order.UserId != request.CurrentUserId)
            throw new UnauthorizedException("error.order_timeline_unauthorized", null);

        // 3. Load status history records
        var statusHistories = await statusHistoryRepository.GetByOrderIdAsync(order.Id, cancellationToken);

        // 4. Load return request if exists
        var returnRequest = await returnRequestRepository.GetByOrderIdAsync(order.Id, cancellationToken);

        // 5. Build timeline events
        var timelineEvents = new List<OrderTimelineEventDto>();

        // Event: Order Created
        timelineEvents.Add(new OrderTimelineEventDto
        {
            Id = Guid.NewGuid(),
            OccurredAt = order.CreatedAt,
            EventType = "Created",
            Title = "Đơn hàng được tạo",
            Description = null,
            ActorName = null,
            Amount = order.TotalAmount
        });

        // Events: Status Changes (from history)
        foreach (var history in statusHistories)
        {
            timelineEvents.Add(new OrderTimelineEventDto
            {
                Id = Guid.NewGuid(),
                OccurredAt = history.ChangedAt,
                EventType = "StatusChanged",
                Title = $"Trạng thái: {FormatEnumName(history.FromStatus.ToString())} → {FormatEnumName(history.ToStatus.ToString())}",
                Description = history.Reason,
                ActorName = history.ChangedBy.HasValue ? "Quản trị viên" : null,
                Amount = null
            });
        }

        // Events: Return Request status changes
        if (returnRequest != null)
        {
            string returnTitle = returnRequest.Status switch
            {
                ReturnStatus.Pending => "Yêu cầu trả hàng được gửi",
                ReturnStatus.Approved => "Yêu cầu trả hàng được phê duyệt",
                ReturnStatus.Rejected => "Yêu cầu trả hàng bị từ chối",
                ReturnStatus.Refunded => "Hoàn tiền thành công",
                _ => "Yêu cầu trả hàng cập nhật"
            };

            timelineEvents.Add(new OrderTimelineEventDto
            {
                Id = Guid.NewGuid(),
                OccurredAt = returnRequest.CreatedAt,
                EventType = "ReturnRequest",
                Title = returnTitle,
                Description = returnRequest.Description,
                ActorName = null,
                Amount = returnRequest.RefundAmount
            });

            if (returnRequest.RefundedAt.HasValue)
            {
                timelineEvents.Add(new OrderTimelineEventDto
                {
                    Id = Guid.NewGuid(),
                    OccurredAt = returnRequest.RefundedAt.Value,
                    EventType = "Refunded",
                    Title = "Hoàn tiền thành công",
                    Description = returnRequest.RefundNote,
                    ActorName = "Quản trị viên",
                    Amount = returnRequest.RefundAmount
                });
            }
        }

        // Event: Delivery
        if (order.DeliveredAt.HasValue)
        {
            timelineEvents.Add(new OrderTimelineEventDto
            {
                Id = Guid.NewGuid(),
                OccurredAt = order.DeliveredAt.Value,
                EventType = "Delivered",
                Title = "Đơn hàng đã được giao",
                Description = null,
                ActorName = null,
                Amount = null
            });
        }

        // 6. Sort by OccurredAt descending (latest first)
        var sortedEvents = timelineEvents
            .OrderByDescending(e => e.OccurredAt)
            .ToList();

        return sortedEvents;
    }

    private static string FormatEnumName(string enumValue)
    {
        return enumValue switch
        {
            "Pending" => "Chờ xác nhận",
            "Confirmed" => "Đã xác nhận",
            "Processing" => "Đang xử lý",
            "Shipped" => "Đã gửi hàng",
            "Delivered" => "Đã giao",
            "Cancelled" => "Đã hủy",
            "Refunded" => "Đã hoàn tiền",
            "Returned" => "Đã trả hàng",
            _ => enumValue
        };
    }
}
