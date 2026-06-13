using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.TitleKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.MessageKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Params)
            .HasMaxLength(2000);

        // Legacy text columns — nullable kể từ khi chuyển sang key-based i18n.
        builder.Property(e => e.Title)
            .HasMaxLength(200);

        builder.Property(e => e.Message)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.UserId, e.IsRead });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
