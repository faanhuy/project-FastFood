using Confluent.Kafka;
using SmartShop.Notification.Application.IntegrationEvents;

namespace SmartShop.Notification.API.Messaging;

/// <summary>
/// Nghe 4 loại event order/payment từ Core và ghi nhận notification (in-app + email + đẩy realtime).
/// Chỉ lo phần Kafka (vòng đọc, commit, retry); việc xử lý nằm ở <see cref="NotificationFanOutEventHandler"/>.
///
/// Kafka giao message "ít nhất một lần" nên có thể nhận trùng; xử lý idempotent theo (SourceOrderId, EventType)
/// nên an toàn. Offset chỉ commit SAU khi xử lý xong — service chết giữa chừng thì message được giao lại.
/// </summary>
public class NotificationFanOutConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationFanOutConsumer> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => RunAsync(stoppingToken), stoppingToken);

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ConsumeSessionAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification fan-out consumer failed. Restarting in {Delay}.", RetryDelay);

                try { await Task.Delay(RetryDelay, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task ConsumeSessionAsync(CancellationToken ct)
    {
        var groupId = configuration["Kafka:GroupId"] ?? "notification-service";
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = groupId,
            // Group mới đọc từ đầu: notification lịch sử chỉ tồn tại trong DB của service này, không có nguồn nào khác.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => logger.LogWarning(
                "Kafka consumer error: {Reason} (code={Code}, fatal={Fatal})", e.Reason, e.Code, e.IsFatal))
            .Build();

        consumer.Subscribe(NotificationFanOutEventHandler.Topics);
        logger.LogInformation(
            "Notification fan-out consumer subscribed to {Topics} (group {GroupId}).",
            string.Join(", ", NotificationFanOutEventHandler.Topics), groupId);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(ct);
                }
                catch (ConsumeException ex) when (!ex.Error.IsFatal)
                {
                    logger.LogWarning(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    await Task.Delay(RetryDelay, ct);
                    continue;
                }

                await ProcessWithRetryAsync(result, ct);
                CommitQuietly(consumer, result);
            }
        }
        finally
        {
            CloseQuietly(consumer);
        }
    }

    private async Task ProcessWithRetryAsync(ConsumeResult<string, string> result, CancellationToken ct)
    {
        while (true)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<NotificationFanOutEventHandler>();
                await handler.HandleAsync(result.Topic, result.Message.Value ?? string.Empty, ct);

                logger.LogInformation(
                    "Handled {Topic} (key={Key}, partition={Partition}, offset={Offset}).",
                    result.Topic, result.Message.Key, result.Partition.Value, result.Offset.Value);
                return;
            }
            catch (InvalidIntegrationEventException ex)
            {
                // Message hỏng: thử lại bao nhiêu lần cũng vậy → bỏ qua để không kẹt partition.
                logger.LogError(ex,
                    "Skipping unprocessable message on {Topic} (key={Key}, partition={Partition}, offset={Offset}).",
                    result.Topic, result.Message.Key, result.Partition.Value, result.Offset.Value);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Lỗi tạm thời (vd DB đang lỗi): thử lại đúng message này, không commit và không đọc message kế tiếp.
                logger.LogWarning(ex,
                    "Failed to handle {Topic} (key={Key}, offset={Offset}). Retrying in {Delay}.",
                    result.Topic, result.Message.Key, result.Offset.Value, RetryDelay);
                await Task.Delay(RetryDelay, ct);
            }
        }
    }

    private void CommitQuietly(IConsumer<string, string> consumer, ConsumeResult<string, string> result)
    {
        try
        {
            consumer.Commit(result);
        }
        catch (KafkaException ex)
        {
            logger.LogWarning(ex,
                "Could not commit offset {Offset} on {Topic}; the message may be delivered again.",
                result.Offset.Value, result.Topic);
        }
    }

    private void CloseQuietly(IConsumer<string, string> consumer)
    {
        try
        {
            consumer.Close();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Kafka consumer did not close cleanly.");
        }
    }
}
