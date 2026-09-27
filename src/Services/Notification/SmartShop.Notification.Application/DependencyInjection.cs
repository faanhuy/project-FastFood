using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartShop.Notification.Application.Features.Notifications.Email;
using SmartShop.Notification.Application.IntegrationEvents;

namespace SmartShop.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddScoped<NotificationFanOutEventHandler>();
        services.AddScoped<NotificationEmailSender>();

        return services;
    }
}
