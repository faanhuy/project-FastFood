using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartShop.Contracts.Events;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Application.Features.Notifications.Email;
using SmartShop.Notification.Domain.Common.Exceptions;
using SmartShop.Notification.Domain.Entities;
using SmartShop.Notification.Domain.Enums;
using SmartShop.Notification.Domain.Interfaces;

namespace SmartShop.Notification.Application.Features.Notifications.Commands.RecordNotification;

public class RecordNotificationCommandHandler(
    INotificationRecordRepository repository,
    IUnitOfWork unitOfWork,
    NotificationEmailSender emailSender,
    ICoreNotifyClient coreNotifyClient,
    IConfiguration configuration,
    ILogger<RecordNotificationCommandHandler> logger) : IRequestHandler<RecordNotificationCommand>
{
    public async Task Handle(RecordNotificationCommand request, CancellationToken ct)
    {
        // Kiểm tra sớm để tránh gửi email trùng ở lần retry thông thường; unique index ở DB
        // (bắt bên dưới) là lớp chặn cuối cho race giữa nhiều consumer instance.
        if (request.SourceOrderId.HasValue &&
            await repository.ExistsAsync(request.SourceOrderId.Value, request.EventType, ct))
        {
            logger.LogInformation(
                "Skipping duplicate notification for order {OrderId}, event {EventType} (already recorded).",
                request.SourceOrderId, request.EventType);
            return;
        }

        var emailPayloadJson =  NotificationEmailSender.TryBuildPayloadJson(request);   

        var record = NotificationRecord.Create(
            request.UserId,
            request.EventType,
            request.TitleKey,
            request.MessageKey,
            request.ParamsJson,
            request.SourceOrderId,
            emailPayloadJson);

        try
        {
            await repository.AddAsync(record, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DuplicateNotificationException)
        {
            logger.LogInformation(
                "Duplicate notification insert caught by unique index for order {OrderId}, event {EventType}.",
                request.SourceOrderId, request.EventType);
            return;
        }

        if (record.EmailStatus == NotificationEmailStatus.Pending)
            await TrySendEmailNowAsync(record, emailPayloadJson!, ct);

        await TryPushRealtimeAsync(record, request, ct);
    }

    // Lần gửi đồng bộ đầu tiên, ngay trong lúc xử lý Kafka message. Thất bại thì lưu trạng thái Failed/backoff
    // (không throw ra ngoài) để EmailRetryBackgroundService tự nhặt lại sau — xem NotificationRecord.RecordEmailFailure.
    private async Task TrySendEmailNowAsync(NotificationRecord record, string emailPayloadJson, CancellationToken ct)
    {
        try
        {
            await emailSender.SendAsync(record.EventType, emailPayloadJson, record.SourceOrderId!.Value, ct);
            record.MarkEmailSent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send notification email for {EventType} (order {OrderId}). Will retry via EmailRetryBackgroundService.",
                record.EventType, record.SourceOrderId);
            record.RecordEmailFailure(ex.Message);
        }

        // Cố tình KHÔNG bọc try/catch quanh đoạn này: nếu chính bước lưu trạng thái lỗi (vd DB tạm thời down),
        // để exception bubble lên khỏi Handle() cho NotificationFanOutConsumer retry ở tầng Kafka message —
        // lần sau ExistsAsync sẽ thấy record đã tồn tại (Pending) và tự phục hồi qua nhánh "stuck-Pending" của
        // GetDueEmailRetriesAsync, không cần xử lý thêm ở đây.
        repository.Update(record);
        await unitOfWork.SaveChangesAsync(ct);
    }

    // Core down/timeout không được làm mất NotificationRecord đã ghi — người dùng vẫn thấy lại khi có REST
    // đọc lịch sử trong sprint sau; sprint này chỉ mất phần toast tức thời.
    private async Task TryPushRealtimeAsync(
        NotificationRecord record, RecordNotificationCommand request, CancellationToken ct)
    {
        try
        {
            await coreNotifyClient.NotifyAsync(
                new InternalNotifyPayload(
                    record.UserId, record.Id, record.TitleKey, record.MessageKey, record.ParamsJson,
                    request.SourceOrderId),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to push realtime notification for {EventType} (order {OrderId}).",
                request.EventType, request.SourceOrderId);
        }
    }
}
