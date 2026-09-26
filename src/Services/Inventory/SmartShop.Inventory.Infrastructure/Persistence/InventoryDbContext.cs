using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.Domain.Common;
using SmartShop.Inventory.Domain.Entities;

namespace SmartShop.Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // UTC bắt buộc: Npgsql chỉ ghi DateTime Kind=Utc vào timestamptz (khác Core lưu giờ VN)
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tự nhận mọi IEntityTypeConfiguration<T> trong assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
