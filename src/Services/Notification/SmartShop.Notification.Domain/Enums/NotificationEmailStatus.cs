namespace SmartShop.Notification.Domain.Enums;

public enum NotificationEmailStatus
{
    NotApplicable = 0,
    Pending = 1,
    Sent = 2,
    Failed = 3,
    DeadLettered = 4
}
