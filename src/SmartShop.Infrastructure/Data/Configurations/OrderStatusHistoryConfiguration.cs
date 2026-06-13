using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.OrderId)
            .IsRequired();

        builder.Property(h => h.FromStatus)
            .IsRequired();

        builder.Property(h => h.ToStatus)
            .IsRequired();

        builder.Property(h => h.ChangedBy)
            .IsRequired(false);

        builder.Property(h => h.Reason)
            .HasMaxLength(500);

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        builder.HasIndex(h => h.OrderId)
            .HasDatabaseName("IX_OrderStatusHistories_OrderId");

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
