using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Persistence.Configurations;

public class OrderArchiveConfiguration : IEntityTypeConfiguration<OrderArchive>
{
    public void Configure(EntityTypeBuilder<OrderArchive> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OriginalOrderId)
            .IsRequired();

        builder.Property(e => e.SnapshotJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ArchivedAt)
            .IsRequired();

        // Index for querying by OriginalOrderId
        builder.HasIndex(e => e.OriginalOrderId);

        // Composite index for archival queries
        builder.HasIndex(e => new { e.CreatedAt, e.ArchivedAt });
    }
}
