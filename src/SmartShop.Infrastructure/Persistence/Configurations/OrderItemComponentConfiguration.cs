using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class OrderItemComponentConfiguration : IEntityTypeConfiguration<OrderItemComponent>
{
    public void Configure(EntityTypeBuilder<OrderItemComponent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ProductImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.SizeLabel)
            .HasMaxLength(100);

        builder.Property(e => e.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.HasOne(e => e.OrderItem)
            .WithMany(oi => oi.Components)
            .HasForeignKey(e => e.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OrderItemId)
            .HasDatabaseName("IX_OrderItemComponents_OrderItemId");
    }
}
