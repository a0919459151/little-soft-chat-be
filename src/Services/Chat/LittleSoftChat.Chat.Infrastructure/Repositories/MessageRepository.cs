using Dapper;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;
using LittleSoftChat.Shared.Infrastructure.Database;

namespace LittleSoftChat.Chat.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(IDbConnectionFactory dbFactory, ILogger<MessageRepository> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<int> CreateAsync(MessageEntity message)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            INSERT INTO messages (sender_id, receiver_id, content, message_type, is_read, created_at)
            VALUES (@SenderId, @ReceiverId, @Content, @MessageType, @IsRead, @CreatedAt);
            SELECT LAST_INSERT_ID();";

        var id = await connection.QuerySingleAsync<int>(sql, message);
        _logger.LogInformation("Created message with ID {MessageId}", id);
        return id;
    }

    public async Task<MessageEntity?> GetByIdAsync(int messageId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT id, sender_id as SenderId, receiver_id as ReceiverId, content, message_type as MessageType, 
                   is_read as IsRead, created_at as CreatedAt, read_at as ReadAt
            FROM messages 
            WHERE id = @MessageId";

        return await connection.QuerySingleOrDefaultAsync<MessageEntity>(sql, new { MessageId = messageId });
    }

    public async Task<PagedResult<MessageDto>> GetMessagesAsync(int userId, int friendId, int page, int size)
    {
        using var connection = _dbFactory.CreateConnection();

        var offset = (page - 1) * size;

        var sql = @"
            SELECT id, sender_id as SenderId, receiver_id as ReceiverId, content, message_type as MessageType, 
                   is_read as IsRead, created_at as CreatedAt, read_at as ReadAt
            FROM messages 
            WHERE (sender_id = @UserId AND receiver_id = @FriendId) 
               OR (sender_id = @FriendId AND receiver_id = @UserId)
            ORDER BY created_at DESC
            LIMIT @Offset, @Size";

        var messages = await connection.QueryAsync<MessageDto>(sql, new
        {
            UserId = userId,
            FriendId = friendId,
            Offset = offset,
            Size = size
        });

        var totalSql = @"
            SELECT COUNT(*)
            FROM messages 
            WHERE (sender_id = @UserId AND receiver_id = @FriendId) 
               OR (sender_id = @FriendId AND receiver_id = @UserId)";

        var total = await connection.QuerySingleAsync<int>(totalSql, new
        {
            UserId = userId,
            FriendId = friendId
        });

        return new PagedResult<MessageDto>(messages.ToList(), page, size, total);
    }

    public async Task<bool> MarkAsReadAsync(int messageId, int userId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            UPDATE messages 
            SET is_read = 1, read_at = @ReadAt
            WHERE id = @MessageId AND receiver_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<List<MessageDto>> GetUnreadMessagesAsync(int userId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT id, sender_id as SenderId, receiver_id as ReceiverId, content, message_type as MessageType, 
                   is_read as IsRead, created_at as CreatedAt, read_at as ReadAt
            FROM messages 
            WHERE receiver_id = @UserId AND is_read = 0
            ORDER BY created_at DESC";

        var messages = await connection.QueryAsync<MessageDto>(sql, new { UserId = userId });
        return messages.ToList();
    }
}
