namespace SmartShop.Shared.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string topic, string key, string payload, CancellationToken ct = default);
}
