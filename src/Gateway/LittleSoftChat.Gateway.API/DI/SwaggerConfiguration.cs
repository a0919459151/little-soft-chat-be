using Microsoft.OpenApi.Models;
using System.Reflection;

namespace LittleSoftChat.Gateway.API.DI;

/// <summary>
/// Swagger 配置擴展
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// 添加 Swagger 配置，包含 JWT 認證支援
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <returns>服務集合</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "LittleSoftChat Gateway API", 
                Version = "v1",
                Description = "LittleSoftChat 應用程式的 Web Gateway API"
            });
            
            // 添加 JWT 認證到 Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
            
            // 包含 XML 註釋
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
        
        return services;
    }
}
