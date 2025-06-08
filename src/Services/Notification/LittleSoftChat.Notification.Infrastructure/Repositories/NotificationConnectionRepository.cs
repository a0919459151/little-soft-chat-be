using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Notification.Domain.Repositories;

namespace LittleSoftChat.Notification.Infrastructure.Repositories;

// 使用內存緩存存儲 SignalR 連接信息
public class NotificationConnectionRepository : INotificationConnectionRepository
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<NotificationConnectionRepository> _logger;
    private const string CONNECTION_KEY_PREFIX = "connections_user_";
    private const string ALL_CONNECTIONS_KEY = "all_connections";

    public NotificationConnectionRepository(IMemoryCache cache, ILogger<NotificationConnectionRepository> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task AddConnectionAsync(string connectionId, int userId)
    {
        try
        {
            var userKey = $"{CONNECTION_KEY_PREFIX}{userId}";
            
            // 獲取用戶現有連接
            var userConnections = _cache.Get<HashSet<string>>(userKey) ?? new HashSet<string>();
            userConnections.Add(connectionId);
            
            // 更新用戶連接列表
            _cache.Set(userKey, userConnections, TimeSpan.FromHours(24));
            
            // 更新全局連接映射
            var allConnections = _cache.Get<Dictionary<string, int>>(ALL_CONNECTIONS_KEY) ?? new Dictionary<string, int>();
            allConnections[connectionId] = userId;
            _cache.Set(ALL_CONNECTIONS_KEY, allConnections, TimeSpan.FromHours(24));
            
            _logger.LogInformation("Added connection {ConnectionId} for user {UserId}", connectionId, userId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add connection {ConnectionId} for user {UserId}", connectionId, userId);
            return Task.CompletedTask;
        }
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        try
        {
            // 從全局連接映射中找到用戶ID
            var allConnections = _cache.Get<Dictionary<string, int>>(ALL_CONNECTIONS_KEY) ?? new Dictionary<string, int>();
            
            if (allConnections.TryGetValue(connectionId, out var userId))
            {
                // 從用戶連接列表中移除
                var userKey = $"{CONNECTION_KEY_PREFIX}{userId}";
                var userConnections = _cache.Get<HashSet<string>>(userKey) ?? new HashSet<string>();
                userConnections.Remove(connectionId);
                
                if (userConnections.Any())
                {
                    _cache.Set(userKey, userConnections, TimeSpan.FromHours(24));
                }
                else
                {
                    _cache.Remove(userKey);
                }
                
                // 從全局映射中移除
                allConnections.Remove(connectionId);
                _cache.Set(ALL_CONNECTIONS_KEY, allConnections, TimeSpan.FromHours(24));
                
                _logger.LogInformation("Removed connection {ConnectionId} for user {UserId}", connectionId, userId);
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove connection {ConnectionId}", connectionId);
            return Task.CompletedTask;
        }
    }

    public Task<List<string>> GetConnectionsByUserIdAsync(int userId)
    {
        try
        {
            var userKey = $"{CONNECTION_KEY_PREFIX}{userId}";
            var connections = _cache.Get<HashSet<string>>(userKey) ?? new HashSet<string>();
            return Task.FromResult(connections.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connections for user {UserId}", userId);
            return Task.FromResult(new List<string>());
        }
    }

    public Task<List<string>> GetConnectionsByUserIdsAsync(IEnumerable<int> userIds)
    {
        try
        {
            var allConnections = new List<string>();
            
            foreach (var userId in userIds)
            {
                var userKey = $"{CONNECTION_KEY_PREFIX}{userId}";
                var connections = _cache.Get<HashSet<string>>(userKey) ?? new HashSet<string>();
                allConnections.AddRange(connections);
            }
            
            return Task.FromResult(allConnections.Distinct().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connections for multiple users");
            return Task.FromResult(new List<string>());
        }
    }

    public Task<bool> IsUserOnlineAsync(int userId)
    {
        try
        {
            var userKey = $"{CONNECTION_KEY_PREFIX}{userId}";
            var connections = _cache.Get<HashSet<string>>(userKey) ?? new HashSet<string>();
            return Task.FromResult(connections.Any());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check online status for user {UserId}", userId);
            return Task.FromResult(false);
        }
    }

    public Task CleanupInactiveConnectionsAsync()
    {
        // 這個方法可以在後台服務中定期調用來清理無效連接
        // 目前使用內存緩存的 TTL 來自動清理
        _logger.LogInformation("Connection cleanup triggered");
        return Task.CompletedTask;
    }
}
