using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using LittleSoftChat.Shared.Infrastructure.Database;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.Behaviors;
using LittleSoftChat.Shared.Infrastructure.Middleware;
using LittleSoftChat.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace LittleSoftChat.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT Authentication
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? ""))
                };
            });

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnectionFactory>(_ => new MySqlConnectionFactory(connectionString!));

        // gRPC Clients
        services.AddGrpcClient<UserService.UserServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:UserService"] ?? "http://localhost:5011");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            
            // 支援 HTTP/2 over HTTP（非加密）
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            
            return handler;
        });
        services.AddScoped<IUserGrpcClient, UserGrpcClient>();

        // MediatR Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }

    public static IApplicationBuilder UseSharedInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<ValidationExceptionMiddleware>();
        return app;
    }
}
