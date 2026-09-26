using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Infrastructure.Persistence;
using SmartShop.Inventory.Infrastructure.Repositories;
using SmartShop.Inventory.Infrastructure.Seeding;

namespace SmartShop.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDb")
            ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:InventoryDb'.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<InventorySeeder>();

        return services;
    }
}
