using LittleSoftChat.Gateway.Presentation.Services;

namespace LittleSoftChat.Gateway.Presentation.DI;

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
        // 目前沒有需要 HTTP 客戶端的服務
        // NotificationService 現在使用 gRPC
        
        return services;
    }
}
