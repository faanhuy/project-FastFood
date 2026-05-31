using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class LocalizedFieldNameConfiguration : IEntityTypeConfiguration<LocalizedFieldName>
{
    public void Configure(EntityTypeBuilder<LocalizedFieldName> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FieldKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(f => f.DisplayName)
            .HasMaxLength(255)
            .IsRequired();

        // Unique constraint on (FieldKey, Language)
        builder.HasIndex(f => new { f.FieldKey, f.Language })
            .IsUnique();

        // For EF auditing
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.CreatedBy);
        builder.Property(f => f.UpdatedAt);
        builder.Property(f => f.UpdatedBy);
    }
}
