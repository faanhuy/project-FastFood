using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReviewId)
            .IsRequired();

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Foreign key: ReviewImage → Review (cascade delete)
        builder.HasOne(e => e.Review)
            .WithMany()
            .HasForeignKey(e => e.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index cho việc query ảnh theo ReviewId
        builder.HasIndex(e => e.ReviewId);

        // Index để sắp xếp theo thứ tự hiển thị
        builder.HasIndex(e => new { e.ReviewId, e.DisplayOrder });
    }
}
