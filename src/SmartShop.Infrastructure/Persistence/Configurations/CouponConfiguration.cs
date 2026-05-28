using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.DiscountType).HasConversion<int>();
        builder.Property(x => x.DiscountValue).HasPrecision(18, 2);
        builder.Property(x => x.MinOrderValue).HasPrecision(18, 2);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Metadata.FindNavigation(nameof(Coupon.Usages))?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
