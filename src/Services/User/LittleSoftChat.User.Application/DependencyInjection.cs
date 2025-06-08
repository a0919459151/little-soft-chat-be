using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.Shared.Infrastructure.Extensions;

namespace LittleSoftChat.User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        // 註冊 MediatR，包含所有 Handler
        services.AddMediatRWithBehaviors(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
