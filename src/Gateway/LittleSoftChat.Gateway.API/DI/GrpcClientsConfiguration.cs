using LittleSoftChat.Shared.Contracts;

namespace LittleSoftChat.Gateway.API.DI;

/// <summary>
/// gRPC 客戶端配置擴展
/// </summary>
public static class GrpcClientsConfiguration
{
    /// <summary>
    /// 添加 gRPC 客戶端配置
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        // User Service gRPC 客戶端
        services.AddGrpcClient<UserService.UserServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcServices:UserService"] ?? "https://localhost:5101");
        });

        // Chat Service gRPC 客戶端
        services.AddGrpcClient<ChatService.ChatServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcServices:ChatService"] ?? "https://localhost:5102");
        });
        
        return services;
    }
}
