using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class OrderFlashSaleUsageConfiguration : IEntityTypeConfiguration<OrderFlashSaleUsage>
{
    public void Configure(EntityTypeBuilder<OrderFlashSaleUsage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.OriginalPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FlashSale)
            .WithMany()
            .HasForeignKey(x => x.FlashSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FlashSaleItem)
            .WithMany()
            .HasForeignKey(x => x.FlashSaleItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
