using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.API.Interceptors;
using SmartShop.Inventory.API.Messaging;
using SmartShop.Inventory.API.Services;
using SmartShop.Inventory.Application;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;
using SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Infrastructure;
using SmartShop.Inventory.Infrastructure.Persistence;
using SmartShop.Inventory.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());
builder.Services.AddGrpcReflection();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// Chỉ nghe Kafka khi có cấu hình broker — không có thì service vẫn chạy độc lập (gRPC) như bình thường
if (!string.IsNullOrWhiteSpace(builder.Configuration["Kafka:BootstrapServers"]))
    builder.Services.AddHostedService<StockReleaseConsumer>();

var app = builder.Build();

// Tự migrate mọi môi trường (giống Core), rồi seed nếu bật cờ Seed:Enabled
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue<bool>("Seed:Enabled"))
        await scope.ServiceProvider.GetRequiredService<InventorySeeder>().SeedAsync();

    // Làm nóng đường xử lý (MediatR pipeline, validator, biên dịch câu truy vấn EF, kết nối DB) để request gRPC đầu tiên
    // không chạm deadline của Core. Cả hai lời gọi chỉ đọc: ReleaseStock cho order chưa từng tồn tại là no-op,
    // GetStockLevel cho sản phẩm không có thì NotFound (bỏ qua).
    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
    try
    {
        await sender.Send(new ReleaseStockCommand(Guid.NewGuid()));

        try { await sender.Send(new GetStockLevelQuery(Guid.NewGuid(), Guid.NewGuid(), null)); }
        catch (NotFoundException) { /* mong đợi */ }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Inventory warm-up failed; the first request may be slow.");
    }
}

app.MapGrpcService<InventoryGrpcService>();

// Reflection cho phép grpcurl/Postman khám phá service mà không cần file .proto
if (app.Configuration.GetValue<bool>("Grpc:EnableReflection"))
    app.MapGrpcReflectionService();

app.Run();
