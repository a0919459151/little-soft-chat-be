using LittleSoftChat.Shared.Infrastructure;

namespace LittleSoftChat.Gateway.API.DI;

/// <summary>
/// Gateway API 中間件配置擴展
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// 配置 Gateway API 中間件管道
    /// </summary>
    /// <param name="app">應用程式建構器</param>
    /// <returns>應用程式建構器</returns>
    public static WebApplication UseGatewayMiddleware(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LittleSoftChat Gateway API v1");
                c.RoutePrefix = string.Empty; // Serve Swagger UI at root
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        // Use shared infrastructure middleware
        app.UseSharedInfrastructure();

        app.MapControllers();

        app.MapGet("/health", () => "Gateway API is running!");
        
        return app;
    }
}
