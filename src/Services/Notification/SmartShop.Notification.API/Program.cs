using Microsoft.EntityFrameworkCore;
using SmartShop.Notification.API.Messaging;
using SmartShop.Notification.Application;
using SmartShop.Notification.Infrastructure;
using SmartShop.Notification.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);

// Chỉ nghe Kafka khi có cấu hình broker — không có thì service vẫn khởi động được (vd chạy migration độc lập)
if (!string.IsNullOrWhiteSpace(builder.Configuration["Kafka:BootstrapServers"]))
    builder.Services.AddHostedService<NotificationFanOutConsumer>();

var app = builder.Build();

// Tự migrate mọi môi trường (giống Inventory Service / Core)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
