using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class CartItemComponentConfiguration : IEntityTypeConfiguration<CartItemComponent>
{
    public void Configure(EntityTypeBuilder<CartItemComponent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.SizeLabel)
            .HasMaxLength(100);

        builder.Property(e => e.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.HasOne(e => e.CartItem)
            .WithMany(ci => ci.Components)
            .HasForeignKey(e => e.CartItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CartItemId)
            .HasDatabaseName("IX_CartItemComponents_CartItemId");
    }
}
