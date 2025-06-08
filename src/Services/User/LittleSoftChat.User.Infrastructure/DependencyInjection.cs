using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.User.Infrastructure.Repositories;

namespace LittleSoftChat.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services)
    {
        // 註冊 Repository
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}
