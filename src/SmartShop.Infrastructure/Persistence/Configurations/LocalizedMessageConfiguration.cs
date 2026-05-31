using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class LocalizedMessageConfiguration : IEntityTypeConfiguration<LocalizedMessage>
{
    public void Configure(EntityTypeBuilder<LocalizedMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MessageKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.MessageText)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.Category)
            .HasMaxLength(50)
            .HasDefaultValue("error");

        // Unique constraint on (MessageKey, Language)
        builder.HasIndex(m => new { m.MessageKey, m.Language })
            .IsUnique();

        // For EF auditing
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy);
        builder.Property(m => m.UpdatedAt);
        builder.Property(m => m.UpdatedBy);
    }
}
