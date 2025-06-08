using LittleSoftChat.Gateway.API.Services;

namespace LittleSoftChat.Gateway.API.DI;

/// <summary>
/// HTTP 客戶端配置擴展
/// </summary>
public static class HttpClientsConfiguration
{
    /// <summary>
    /// 添加 HTTP 客戶端配置
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Notification Service HTTP 客戶端
        services.AddHttpClient<INotificationService, NotificationService>(client =>
        {
            client.BaseAddress = new Uri(configuration["HttpServices:NotificationService"] ?? "https://localhost:5103");
        });
        
        return services;
    }
}
