using LittleSoftChat.Gateway.Presentation.Services;
using LittleSoftChat.Shared.Infrastructure;

namespace LittleSoftChat.Gateway.Presentation.DI;

/// <summary>
/// Gateway API 依賴注入配置的主要入口點
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 註冊 Gateway API 的所有服務
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加控制器和API相關服務
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        // 添加基礎設施服務
        services.AddSwaggerConfiguration();
        services.AddCorsConfiguration();
        
        // 添加共享基礎設施 (JWT, Database, etc.)
        services.AddSharedInfrastructure(configuration);
        
        // 添加 gRPC 客戶端
        services.AddGrpcClients(configuration);
        
        // 添加 HTTP 客戶端
        services.AddHttpClients(configuration);
        
        // 註冊 Gateway 服務
        services.AddGatewayApplicationServices();
        
        return services;
    }
}
