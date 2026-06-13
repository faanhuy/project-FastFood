using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        // Store enum as int (default EF behaviour — explicit for clarity)
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        // Performance indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.UserId, e.Status });
        builder.HasIndex(e => e.IsArchived);

        // Unique filtered index for VnpayTransactionId
        builder.HasIndex(e => e.VnpayTransactionId)
            .HasFilter("[VnpayTransactionId] IS NOT NULL")
            .IsUnique();

        // many:1 Order → User (restrict: don't delete user with orders)
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // many:1 Order → Store (nullable — set null when store deleted)
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // 1:many Order → OrderItems
        // Order has private backing field _items — EF finds it by naming convention
        builder.HasMany(e => e.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
