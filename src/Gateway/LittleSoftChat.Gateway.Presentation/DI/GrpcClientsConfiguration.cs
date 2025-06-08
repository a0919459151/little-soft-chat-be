using LittleSoftChat.Shared.Contracts;

namespace LittleSoftChat.Gateway.Presentation.DI;

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
            options.Address = new Uri(configuration["GrpcServices:UserService"] ?? "http://localhost:5001");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            
            // 在 Docker 環境中，gRPC 服務使用 HTTP/2 over HTTP（非 HTTPS）
            var grpcServiceUrl = configuration["GrpcServices:UserService"];
            if (!string.IsNullOrEmpty(grpcServiceUrl) && grpcServiceUrl.StartsWith("http://"))
            {
                // HTTP/2 over HTTP 配置
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }
            else
            {
                // HTTPS 配置：允許自簽證書
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            
            return handler;
        });

        // Chat Service gRPC 客戶端
        services.AddGrpcClient<ChatService.ChatServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcServices:ChatService"] ?? "http://localhost:5215");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            
            // 在 Docker 環境中，gRPC 服務使用 HTTP/2 over HTTP（非 HTTPS）
            var grpcServiceUrl = configuration["GrpcServices:ChatService"];
            if (!string.IsNullOrEmpty(grpcServiceUrl) && grpcServiceUrl.StartsWith("http://"))
            {
                // HTTP/2 over HTTP 配置
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }
            else
            {
                // HTTPS 配置：允許自簽證書
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            
            return handler;
        });

        // Notification Service gRPC 客戶端
        services.AddGrpcClient<NotificationService.NotificationServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcServices:NotificationService"] ?? "http://localhost:5013");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            
            // 在 Docker 環境中，gRPC 服務使用 HTTP/2 over HTTP（非 HTTPS）
            var grpcServiceUrl = configuration["GrpcServices:NotificationService"];
            if (!string.IsNullOrEmpty(grpcServiceUrl) && grpcServiceUrl.StartsWith("http://"))
            {
                // HTTP/2 over HTTP 配置
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }
            else
            {
                // HTTPS 配置：允許自簽證書
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            
            return handler;
        });
        
        return services;
    }
}
