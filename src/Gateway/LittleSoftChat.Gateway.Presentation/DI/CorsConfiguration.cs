namespace LittleSoftChat.Gateway.Presentation.DI;

/// <summary>
/// CORS 配置擴展
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// 添加 CORS 配置
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        return services;
    }
}
