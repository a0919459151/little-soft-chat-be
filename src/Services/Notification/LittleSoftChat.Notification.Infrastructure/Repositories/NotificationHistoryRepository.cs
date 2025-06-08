using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Notification.Domain.Repositories;
using LittleSoftChat.Notification.Domain.Entities;

namespace LittleSoftChat.Notification.Infrastructure.Repositories;

public class NotificationHistoryRepository : INotificationHistoryRepository
{
    private readonly string _connectionString;
    private readonly ILogger<NotificationHistoryRepository> _logger;

    public NotificationHistoryRepository(IConfiguration configuration, ILogger<NotificationHistoryRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<int> CreateAsync(NotificationHistoryEntity notification)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                INSERT INTO notification_history (user_id, type, title, content, is_read, created_at)
                VALUES (@UserId, @Type, @Title, @Content, @IsRead, @CreatedAt);
                SELECT LAST_INSERT_ID();";

            var id = await connection.QuerySingleAsync<int>(sql, new
            {
                notification.UserId,
                notification.Type,
                notification.Title,
                notification.Content,
                notification.IsRead,
                notification.CreatedAt
            });

            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", id, notification.UserId);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for user {UserId}", notification.UserId);
            throw;
        }
    }

    public async Task<List<NotificationHistoryEntity>> GetByUserIdAsync(int userId, int page = 1, int size = 20)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                SELECT id, user_id as UserId, type, title, content, is_read as IsRead, created_at as CreatedAt, read_at as ReadAt
                FROM notification_history 
                WHERE user_id = @UserId 
                ORDER BY created_at DESC 
                LIMIT @Limit OFFSET @Offset";

            var offset = (page - 1) * size;
            
            var notifications = await connection.QueryAsync<NotificationHistoryEntity>(sql, new
            {
                UserId = userId,
                Limit = size,
                Offset = offset
            });

            return notifications.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
            return new List<NotificationHistoryEntity>();
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM notification_history 
                WHERE user_id = @UserId AND is_read = FALSE";

            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                UPDATE notification_history 
                SET is_read = TRUE, read_at = @ReadAt 
                WHERE id = @NotificationId AND user_id = @UserId";

            await connection.ExecuteAsync(sql, new
            {
                NotificationId = notificationId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
            throw;
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                UPDATE notification_history 
                SET is_read = TRUE, read_at = @ReadAt 
                WHERE user_id = @UserId AND is_read = FALSE";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationHistoryEntity?> GetByIdAsync(int id)
    {
        try
        {
            using var connection = CreateConnection();
            
            const string sql = @"
                SELECT id, user_id as UserId, type, title, content, is_read as IsRead, created_at as CreatedAt, read_at as ReadAt
                FROM notification_history 
                WHERE id = @Id";

            return await connection.QuerySingleOrDefaultAsync<NotificationHistoryEntity>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification {NotificationId}", id);
            return null;
        }
    }
}
