using LittleSoftChat.Notification.Domain.Entities;

namespace LittleSoftChat.Notification.Domain.Repositories;

public interface INotificationConnectionRepository
{
    Task AddConnectionAsync(string connectionId, int userId);
    Task RemoveConnectionAsync(string connectionId);
    Task<List<string>> GetConnectionsByUserIdAsync(int userId);
    Task<List<string>> GetConnectionsByUserIdsAsync(IEnumerable<int> userIds);
    Task<bool> IsUserOnlineAsync(int userId);
    Task CleanupInactiveConnectionsAsync();
}

public interface INotificationHistoryRepository
{
    Task<int> CreateAsync(NotificationHistoryEntity notification);
    Task<List<NotificationHistoryEntity>> GetByUserIdAsync(int userId, int page = 1, int size = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task<NotificationHistoryEntity?> GetByIdAsync(int id);
}
