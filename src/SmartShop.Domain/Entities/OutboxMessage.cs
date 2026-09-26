using SmartShop.Domain.Common;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    private OutboxMessage() { }

    public string EventType { get; private set; } = string.Empty;
    public string AggregateKey { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public OutboxStatus Status { get; private set; } = OutboxStatus.Pending;
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    public static OutboxMessage Create(string eventType, string aggregateKey, string payload, DateTime occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new OutboxMessage
        {
            EventType = eventType,
            AggregateKey = aggregateKey,
            Payload = payload,
            OccurredAt = occurredAt,
            Status = OutboxStatus.Pending
        };
    }

    public void MarkPublished(DateTime processedAt)
    {
        Status = OutboxStatus.Published;
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        Error = error;
    }
}
