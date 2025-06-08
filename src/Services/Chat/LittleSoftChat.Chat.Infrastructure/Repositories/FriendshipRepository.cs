using Dapper;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Infrastructure.Database;

namespace LittleSoftChat.Chat.Infrastructure.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<FriendshipRepository> _logger;

    public FriendshipRepository(IDbConnectionFactory dbFactory, ILogger<FriendshipRepository> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<int> CreateAsync(FriendshipEntity friendship)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            INSERT INTO friendships (user_id, friend_id, status, created_at)
            VALUES (@UserId, @FriendId, @Status, @CreatedAt);
            SELECT LAST_INSERT_ID();";

        var id = await connection.QuerySingleAsync<int>(sql, friendship);
        _logger.LogInformation("Created friendship with ID {FriendshipId}", id);
        return id;
    }

    public async Task<FriendshipEntity?> GetByIdAsync(int friendshipId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT id, user_id as UserId, friend_id as FriendId, status, created_at as CreatedAt, accepted_at as AcceptedAt
            FROM friendships 
            WHERE id = @FriendshipId";

        return await connection.QuerySingleOrDefaultAsync<FriendshipEntity>(sql, new { FriendshipId = friendshipId });
    }

    public async Task<FriendshipEntity?> GetByUsersAsync(int userId1, int userId2)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT id, user_id as UserId, friend_id as FriendId, status, created_at as CreatedAt, accepted_at as AcceptedAt
            FROM friendships 
            WHERE (user_id = @UserId1 AND friend_id = @UserId2) 
               OR (user_id = @UserId2 AND friend_id = @UserId1)";

        return await connection.QuerySingleOrDefaultAsync<FriendshipEntity>(sql, new 
        { 
            UserId1 = userId1, 
            UserId2 = userId2 
        });
    }

    public async Task<bool> UpdateStatusAsync(int friendshipId, FriendshipStatus status)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            UPDATE friendships 
            SET status = @Status, accepted_at = @AcceptedAt
            WHERE id = @FriendshipId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            FriendshipId = friendshipId,
            Status = status,
            AcceptedAt = status == FriendshipStatus.Accepted ? DateTime.UtcNow : (DateTime?)null
        });

        return rowsAffected > 0;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(int userId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT 
                CASE 
                    WHEN f.user_id = @UserId THEN f.friend_id 
                    ELSE f.user_id 
                END as Id,
                CASE 
                    WHEN f.user_id = @UserId THEN fu.username 
                    ELSE uu.username 
                END as Username,
                CASE 
                    WHEN f.user_id = @UserId THEN fu.display_name 
                    ELSE uu.display_name 
                END as DisplayName,
                CASE 
                    WHEN f.user_id = @UserId THEN fu.avatar 
                    ELSE uu.avatar 
                END as Avatar,
                CASE 
                    WHEN f.user_id = @UserId THEN fu.is_active 
                    ELSE uu.is_active 
                END as IsActive,
                f.accepted_at as FriendsSince
            FROM friendships f
            JOIN users uu ON uu.id = f.user_id
            JOIN users fu ON fu.id = f.friend_id
            WHERE (f.user_id = @UserId OR f.friend_id = @UserId) 
              AND f.status = @AcceptedStatus";

        var friends = await connection.QueryAsync<FriendDto>(sql, new 
        { 
            UserId = userId,
            AcceptedStatus = FriendshipStatus.Accepted
        });

        return friends.ToList();
    }

    public async Task<List<FriendRequestDto>> GetFriendRequestsAsync(int userId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            SELECT 
                f.id as RequestId,
                u.id as SenderId,
                u.username as SenderUsername,
                u.display_name as SenderDisplayName,
                u.avatar as SenderAvatar,
                f.created_at as CreatedAt
            FROM friendships f
            JOIN users u ON u.id = f.user_id
            WHERE f.friend_id = @UserId 
              AND f.status = @PendingStatus
            ORDER BY f.created_at DESC";

        var requests = await connection.QueryAsync<FriendRequestDto>(sql, new 
        { 
            UserId = userId,
            PendingStatus = FriendshipStatus.Pending
        });

        return requests.ToList();
    }

    public async Task<bool> RemoveFriendshipAsync(int userId, int friendId)
    {
        using var connection = _dbFactory.CreateConnection();

        var sql = @"
            DELETE FROM friendships 
            WHERE (user_id = @UserId AND friend_id = @FriendId) 
               OR (user_id = @FriendId AND friend_id = @UserId)";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            FriendId = friendId
        });

        return rowsAffected > 0;
    }
}
