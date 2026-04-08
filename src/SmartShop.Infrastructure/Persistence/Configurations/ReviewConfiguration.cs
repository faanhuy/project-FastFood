using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rating)
            .IsRequired();

        builder.Property(e => e.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.IsApproved)
            .HasDefaultValue(false);

        // many:1 Review → User (restrict: preserve reviews when user deleted)
        // Product → Reviews relationship is configured in ProductConfiguration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
