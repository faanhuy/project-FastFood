using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Contracts.Events;
using SmartShop.Domain.Interfaces;
using SmartShop.Shared.Messaging;

namespace SmartShop.Infrastructure.Messaging;

public class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher loop failed");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }

    private async Task PublishPendingBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await outboxRepository.GetPendingBatchAsync(BatchSize, ct);
        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            try
            {
                var topic = ResolveTopic(message.EventType);
                await eventPublisher.PublishAsync(topic, message.AggregateKey, message.Payload, ct);
                message.MarkPublished(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to publish outbox message {MessageId} ({EventType})", message.Id, message.EventType);
                message.MarkFailed(ex.Message);
            }

            outboxRepository.Update(message);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static string ResolveTopic(string eventType) => eventType switch
    {
        nameof(OrderPlacedIntegrationEvent) => "smartshop.order.placed",
        nameof(OrderCancelledIntegrationEvent) => "smartshop.order.cancelled",
        nameof(PaymentCompletedIntegrationEvent) => "smartshop.payment.completed",
        nameof(PaymentFailedIntegrationEvent) => "smartshop.payment.failed",
        _ => throw new InvalidOperationException($"Unknown outbox event type: {eventType}")
    };
}
