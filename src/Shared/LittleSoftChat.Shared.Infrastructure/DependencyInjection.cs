using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using LittleSoftChat.Shared.Infrastructure.Database;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.HttpClients;
using LittleSoftChat.Shared.Contracts;

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
            options.Address = new Uri(configuration["GrpcServices:UserService"] ?? "https://localhost:5101");
        });
        services.AddScoped<IUserGrpcClient, UserGrpcClient>();

        // HTTP Clients
        services.AddHttpClient<INotificationHttpClient, NotificationHttpClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["HttpServices:NotificationService"] ?? "https://localhost:5103");
        });

        return services;
    }
}
