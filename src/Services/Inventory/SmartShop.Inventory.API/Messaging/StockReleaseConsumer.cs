using Confluent.Kafka;
using FluentValidation;
using SmartShop.Inventory.Application.IntegrationEvents;

namespace SmartShop.Inventory.API.Messaging;

/// <summary>
/// Nghe event hủy đơn từ Core và nhả chỗ đã giữ. Chỉ lo phần Kafka (vòng đọc, commit, retry);
/// việc nhả chỗ nằm ở <see cref="StockReleaseEventHandler"/>.
///
/// Kafka giao message "ít nhất một lần" nên có thể nhận trùng; nhả chỗ idempotent theo order_id nên an toàn.
/// Offset chỉ commit SAU khi xử lý xong — service chết giữa chừng thì message được giao lại.
/// </summary>
public class StockReleaseConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<StockReleaseConsumer> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    // Consume() chặn thread nên chạy trên thread pool riêng, không giữ lại quá trình khởi động của host.
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
                // Không để exception thoát ra: BackgroundService ném lỗi sẽ dừng cả host, kéo theo gRPC ngừng phục vụ.
                logger.LogError(ex, "Stock release consumer failed. Restarting in {Delay}.", RetryDelay);

                try { await Task.Delay(RetryDelay, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task ConsumeSessionAsync(CancellationToken ct)
    {
        var groupId = configuration["Kafka:GroupId"] ?? "inventory-service";
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = groupId,
            // Group mới đọc từ đầu: event cũ của đơn chưa từng giữ chỗ chỉ là no-op, còn đơn đã giữ chỗ thì cần nhả.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            // Consumer khởi động trước producer vẫn subscribe được (topic tự tạo khi broker cho phép).
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => logger.LogWarning(
                "Kafka consumer error: {Reason} (code={Code}, fatal={Fatal})", e.Reason, e.Code, e.IsFatal))
            .Build();

        consumer.Subscribe(StockReleaseEventHandler.Topics);
        logger.LogInformation(
            "Stock release consumer subscribed to {Topics} (group {GroupId}).",
            string.Join(", ", StockReleaseEventHandler.Topics), groupId);

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
                var handler = scope.ServiceProvider.GetRequiredService<StockReleaseEventHandler>();
                await handler.HandleAsync(result.Topic, result.Message.Value ?? string.Empty, ct);

                logger.LogInformation(
                    "Handled {Topic} (key={Key}, partition={Partition}, offset={Offset}).",
                    result.Topic, result.Message.Key, result.Partition.Value, result.Offset.Value);
                return;
            }
            catch (Exception ex) when (ex is InvalidIntegrationEventException or ValidationException)
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
            // Nhả chỗ idempotent nên message được giao lại sau rebalance cũng không sao.
            logger.LogWarning(ex,
                "Could not commit offset {Offset} on {Topic}; the message may be delivered again.",
                result.Offset.Value, result.Topic);
        }
    }

    private void CloseQuietly(IConsumer<string, string> consumer)
    {
        try
        {
            // Commit offset cuối và rời group gọn gàng để partition được chia lại ngay.
            consumer.Close();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Kafka consumer did not close cleanly.");
        }
    }
}
