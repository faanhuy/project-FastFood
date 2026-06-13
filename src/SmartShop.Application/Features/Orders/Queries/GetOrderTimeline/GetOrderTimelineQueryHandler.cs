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
            Title = "timeline.created",
            TitleKey = "timeline.created",
            IsAdminActor = false,
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
                Title = "timeline.statusChanged",
                TitleKey = "timeline.statusChanged",
                FromStatus = history.FromStatus.ToString(),
                ToStatus = history.ToStatus.ToString(),
                IsAdminActor = history.ChangedBy.HasValue,
                Description = history.Reason,
                ActorName = history.ChangedBy.HasValue ? "admin" : null,
                Amount = null
            });
        }

        // Events: Return Request status changes
        if (returnRequest != null)
        {
            string returnTitleKey = returnRequest.Status switch
            {
                ReturnStatus.Pending => "timeline.returnPending",
                ReturnStatus.Approved => "timeline.returnApproved",
                ReturnStatus.Rejected => "timeline.returnRejected",
                ReturnStatus.Refunded => "timeline.refunded",
                _ => "timeline.returnUpdated"
            };

            timelineEvents.Add(new OrderTimelineEventDto
            {
                Id = Guid.NewGuid(),
                OccurredAt = returnRequest.CreatedAt,
                EventType = "ReturnRequest",
                Title = returnTitleKey,
                TitleKey = returnTitleKey,
                IsAdminActor = false,
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
                    Title = "timeline.refunded",
                    TitleKey = "timeline.refunded",
                    IsAdminActor = true,
                    Description = returnRequest.RefundNote,
                    ActorName = "admin",
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
                Title = "timeline.delivered",
                TitleKey = "timeline.delivered",
                IsAdminActor = false,
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

}
