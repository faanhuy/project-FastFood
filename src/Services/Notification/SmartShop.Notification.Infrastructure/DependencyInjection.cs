using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Domain.Interfaces;
using SmartShop.Notification.Infrastructure.BackgroundServices;
using SmartShop.Notification.Infrastructure.Email;
using SmartShop.Notification.Infrastructure.Http;
using SmartShop.Notification.Infrastructure.Persistence;
using SmartShop.Notification.Infrastructure.Repositories;

namespace SmartShop.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:NotificationDb'.");

        services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<INotificationRecordRepository, NotificationRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        var coreBaseUrl = configuration["Internal:CoreBaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration 'Internal:CoreBaseUrl'.");

        services.AddHttpClient<ICoreNotifyClient, CoreNotifyHttpClient>(client =>
        {
            client.BaseAddress = new Uri(coreBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHostedService<EmailRetryBackgroundService>();

        return services;
    }
}
