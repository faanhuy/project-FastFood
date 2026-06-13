using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class FlashSaleItemConfiguration : IEntityTypeConfiguration<FlashSaleItem>
{
    public void Configure(EntityTypeBuilder<FlashSaleItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FlashSaleId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.SizeId).IsRequired(false);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.OriginalPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.StockLimit).IsRequired();
        builder.Property(x => x.SoldCount).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductSize)
            .WithMany()
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.FlashSaleId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.SizeId);
    }
}
