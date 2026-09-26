using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.API.Interceptors;
using SmartShop.Inventory.API.Services;
using SmartShop.Inventory.Application;
using SmartShop.Inventory.Infrastructure;
using SmartShop.Inventory.Infrastructure.Persistence;
using SmartShop.Inventory.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());
builder.Services.AddGrpcReflection();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

var app = builder.Build();

// Tự migrate mọi môi trường (giống Core), rồi seed nếu bật cờ Seed:Enabled
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue<bool>("Seed:Enabled"))
        await scope.ServiceProvider.GetRequiredService<InventorySeeder>().SeedAsync();
}

app.MapGrpcService<InventoryGrpcService>();

// Reflection cho phép grpcurl/Postman khám phá service mà không cần file .proto
if (app.Configuration.GetValue<bool>("Grpc:EnableReflection"))
    app.MapGrpcReflectionService();

app.Run();
