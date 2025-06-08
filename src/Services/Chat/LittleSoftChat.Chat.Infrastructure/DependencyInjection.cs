using Microsoft.Extensions.DependencyInjection;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Chat.Infrastructure.Repositories;

namespace LittleSoftChat.Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatInfrastructure(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();

        return services;
    }
}
