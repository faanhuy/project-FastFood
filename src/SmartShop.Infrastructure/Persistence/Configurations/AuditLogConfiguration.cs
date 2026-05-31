using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasMaxLength(100);

        builder.Property(a => a.IPAddress)
            .HasMaxLength(45);

        // Indexes for querying
        builder.HasIndex(a => new { a.UserId, a.Timestamp });
        builder.HasIndex(a => new { a.Action, a.Timestamp });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
