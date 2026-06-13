namespace SmartShop.Application.Features.Orders;

public class OrderTimelineEventDto
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActorName { get; set; }
    public decimal? Amount { get; set; }
}
