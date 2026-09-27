using MediatR;
using SmartShop.Notification.Application.Common.Interfaces;

namespace SmartShop.Notification.Application.Features.Notifications.Commands.RecordNotification;

// EventType dùng nameof(<...>IntegrationEvent) từ SmartShop.Contracts để 1-1 với message key i18n phía FE
// (xem .claude/rules/i18n.md — TitleKey/MessageKey không hardcode text, FE tự dịch theo locale).
public record RecordNotificationCommand(
    Guid UserId,
    string EventType,
    string TitleKey,
    string MessageKey,
    string? ParamsJson,
    Guid? SourceOrderId,
    string? UserEmail,
    string? UserName,
    decimal? Amount,
    string? PaymentMethod,
    string? Reason,
    string? OrderNumber = null,
    List<NotificationOrderItemInfo>? Items = null) : IRequest;
