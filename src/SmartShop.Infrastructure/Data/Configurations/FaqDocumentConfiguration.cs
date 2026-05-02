using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data.Configurations;

public class FaqDocumentConfiguration : IEntityTypeConfiguration<FaqDocument>
{
    public void Configure(EntityTypeBuilder<FaqDocument> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.Answer)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.IsActive)
            .IsRequired();

        builder.HasIndex(f => f.Category);
        builder.HasIndex(f => f.IsActive);
    }
}
