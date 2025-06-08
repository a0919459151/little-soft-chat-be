using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Notification.Domain.Repositories;
using LittleSoftChat.Notification.Domain.Entities;
using LittleSoftChat.Notification.Application.Hubs;

namespace LittleSoftChat.Notification.Application.Services;

public interface INotificationService
{
    Task SendRealtimeNotificationAsync(int userId, string type, string title, string content, object? data = null);
    Task SendSystemNotificationAsync(List<int> userIds, string title, string content);
    Task<bool> IsUserOnlineAsync(int userId);
    Task<List<NotificationHistoryEntity>> GetUserNotificationsAsync(int userId, int page = 1, int size = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly INotificationConnectionRepository _connectionRepository;
    private readonly INotificationHistoryRepository _historyRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<ChatHub> hubContext,
        INotificationConnectionRepository connectionRepository,
        INotificationHistoryRepository historyRepository,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _connectionRepository = connectionRepository;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task SendRealtimeNotificationAsync(int userId, string type, string title, string content, object? data = null)
    {
        try
        {
            // 保存到歷史記錄（僅當類型為 message 或 system 時）
            if (type == "message" || type == "system")
            {
                var notification = new NotificationHistoryEntity
                {
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Content = content,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _historyRepository.CreateAsync(notification);
            }

            // 檢查用戶是否在線
            var isOnline = await IsUserOnlineAsync(userId);
            if (isOnline)
            {
                // 發送即時通知
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        Type = type,
                        Title = title,
                        Content = content,
                        Data = data,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("Sent realtime notification to user {UserId}, type: {Type}", userId, type);
            }
            else
            {
                _logger.LogInformation("User {UserId} is offline, notification saved to history only", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send realtime notification to user {UserId}", userId);
            throw;
        }
    }

    public async Task SendSystemNotificationAsync(List<int> userIds, string title, string content)
    {
        try
        {
            // 保存到所有用戶的歷史記錄
            var tasks = userIds.Select(async userId =>
            {
                var notification = new NotificationHistoryEntity
                {
                    UserId = userId,
                    Type = "system",
                    Title = title,
                    Content = content,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _historyRepository.CreateAsync(notification);
            });

            await Task.WhenAll(tasks);

            // 獲取在線用戶的連接
            var onlineConnections = await _connectionRepository.GetConnectionsByUserIdsAsync(userIds);
            
            if (onlineConnections.Any())
            {
                // 發送給所有在線用戶
                var groups = userIds.Select(id => $"User_{id}").ToArray();
                await _hubContext.Clients.Groups(groups)
                    .SendAsync("ReceiveNotification", new
                    {
                        Type = "system",
                        Title = title,
                        Content = content,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("Sent system notification to {UserCount} users", userIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system notification to users");
            throw;
        }
    }

    public async Task<bool> IsUserOnlineAsync(int userId)
    {
        return await _connectionRepository.IsUserOnlineAsync(userId);
    }

    public async Task<List<NotificationHistoryEntity>> GetUserNotificationsAsync(int userId, int page = 1, int size = 20)
    {
        return await _historyRepository.GetByUserIdAsync(userId, page, size);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _historyRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        await _historyRepository.MarkAsReadAsync(notificationId, userId);
        
        // 通知前端更新未讀數量
        var unreadCount = await GetUnreadCountAsync(userId);
        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("UnreadCountUpdated", unreadCount);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _historyRepository.MarkAllAsReadAsync(userId);
        
        // 通知前端更新未讀數量
        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("UnreadCountUpdated", 0);
    }
}
