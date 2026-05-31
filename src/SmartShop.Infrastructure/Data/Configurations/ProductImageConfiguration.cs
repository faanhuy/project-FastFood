using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.SortOrder)
            .HasDefaultValue(0);

        builder.HasOne<Product>()
            .WithMany(p => p.Images)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("ProductImages");
    }
}
