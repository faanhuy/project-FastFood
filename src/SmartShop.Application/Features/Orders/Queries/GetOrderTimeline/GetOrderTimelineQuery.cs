using MediatR;

namespace SmartShop.Application.Features.Orders.Queries.GetOrderTimeline;

public record GetOrderTimelineQuery(
    Guid OrderId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<List<OrderTimelineEventDto>>;
