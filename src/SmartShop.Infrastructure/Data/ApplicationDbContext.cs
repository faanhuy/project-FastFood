using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.OriginalPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.HasIndex(e => e.Slug)
                .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500);

            entity.Property(e => e.RefreshTokenExpiry);
        });
    }
}
