using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Notification.Domain.Repositories;

namespace LittleSoftChat.Notification.Application.Services;

public class ConnectionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConnectionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // 每5分鐘清理一次

    public ConnectionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ConnectionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var connectionRepository = scope.ServiceProvider.GetRequiredService<INotificationConnectionRepository>();
                
                await connectionRepository.CleanupInactiveConnectionsAsync();
                _logger.LogInformation("Connection cleanup completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during connection cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
