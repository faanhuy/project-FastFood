using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(x => x.RejectedReason)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.IsActive, x.EndAt });
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.FlashSale)
            .HasForeignKey(i => i.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
