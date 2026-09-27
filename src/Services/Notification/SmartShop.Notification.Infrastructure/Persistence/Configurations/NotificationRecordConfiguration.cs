using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Notification.Domain.Entities;

namespace SmartShop.Notification.Infrastructure.Persistence.Configurations;

public class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
{
    public void Configure(EntityTypeBuilder<NotificationRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TitleKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MessageKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ParamsJson).HasColumnType("nvarchar(max)");

        builder.Property(x => x.EmailStatus).HasConversion<int>();
        builder.Property(x => x.EmailLastError).HasMaxLength(2000);
        builder.Property(x => x.EmailPayloadJson).HasColumnType("nvarchar(max)");

        // Filtered unique index: nhiều event không gắn với order (SourceOrderId null) không bị chặn lẫn nhau,
        // chỉ chặn trùng lặp thật sự (cùng đơn, cùng loại event) khi Kafka giao lại message.
        builder.HasIndex(x => new { x.SourceOrderId, x.EventType })
            .IsUnique()
            .HasFilter("[SourceOrderId] IS NOT NULL");

        // Filtered index chỉ gồm Pending/Failed (loại Sent/NotApplicable/DeadLettered) — giữ kích thước nhỏ
        // dù bảng lớn dần, phục vụ EmailRetryBackgroundService quét định kỳ.
        builder.HasIndex(x => new { x.EmailStatus, x.NextEmailRetryAt })
            .HasDatabaseName("IX_NotificationRecords_EmailRetry")
            .HasFilter("[EmailStatus] IN (1, 3)");
    }
}
