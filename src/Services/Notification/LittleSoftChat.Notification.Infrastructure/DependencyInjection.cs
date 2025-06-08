using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.Notification.Domain.Repositories;
using LittleSoftChat.Notification.Infrastructure.Repositories;

namespace LittleSoftChat.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // 註冊倉儲
        services.AddScoped<INotificationConnectionRepository, NotificationConnectionRepository>();
        services.AddScoped<INotificationHistoryRepository, NotificationHistoryRepository>();
        
        // 添加內存緩存用於連接管理
        services.AddMemoryCache();
        
        return services;
    }
}
