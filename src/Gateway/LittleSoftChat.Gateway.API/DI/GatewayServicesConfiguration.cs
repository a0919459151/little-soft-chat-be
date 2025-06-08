using LittleSoftChat.Gateway.API.Services;

namespace LittleSoftChat.Gateway.API.DI;

/// <summary>
/// Gateway 應用服務配置擴展
/// </summary>
public static class GatewayServicesConfiguration
{
    /// <summary>
    /// 註冊 Gateway 應用服務
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddGatewayApplicationServices(this IServiceCollection services)
    {
        // 註冊 Gateway 服務
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        return services;
    }
}
