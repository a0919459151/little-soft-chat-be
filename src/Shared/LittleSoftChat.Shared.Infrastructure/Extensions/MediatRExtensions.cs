using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.Shared.Infrastructure.Behaviors;

namespace LittleSoftChat.Shared.Infrastructure.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, Assembly assembly)
    {
        // 註冊 MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // 註冊 Pipeline Behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        return services;
    }
}
