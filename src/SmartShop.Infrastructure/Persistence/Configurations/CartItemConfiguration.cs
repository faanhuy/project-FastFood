using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ItemType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.SizeLabel)
            .HasMaxLength(20);

        builder.Ignore(e => e.SubTotal);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Components)
            .WithOne(c => c.CartItem)
            .HasForeignKey(c => c.CartItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
