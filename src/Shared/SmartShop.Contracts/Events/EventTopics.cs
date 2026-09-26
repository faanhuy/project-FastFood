namespace SmartShop.Contracts.Events;

/// <summary>
/// Tên topic Kafka của các integration event. Producer (Core) và consumer (các service khác) cùng dùng hằng số
/// này để không lệch tên — sai 1 ký tự thì consumer subscribe topic không bao giờ có message.
/// </summary>
public static class EventTopics
{
    public const string OrderPlaced = "smartshop.order.placed";
    public const string OrderCancelled = "smartshop.order.cancelled";
    public const string PaymentCompleted = "smartshop.payment.completed";
    public const string PaymentFailed = "smartshop.payment.failed";
}
