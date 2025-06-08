using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Register Application Services
        services.AddScoped<INotificationService, NotificationService>();
        
        // Register Background Services
        services.AddHostedService<ConnectionCleanupService>();
        
        return services;
    }
}
