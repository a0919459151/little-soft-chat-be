using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Chat.Domain.Repositories;

public interface IMessageRepository
{
    Task<int> CreateAsync(MessageEntity message);
    Task<MessageEntity?> GetByIdAsync(int messageId);
    Task<PagedResult<MessageDto>> GetMessagesAsync(int userId, int friendId, int page, int size);
    Task<bool> MarkAsReadAsync(int messageId, int userId);
    Task<List<MessageDto>> GetUnreadMessagesAsync(int userId);
}

public interface IFriendshipRepository
{
    Task<int> CreateAsync(FriendshipEntity friendship);
    Task<FriendshipEntity?> GetByIdAsync(int friendshipId);
    Task<FriendshipEntity?> GetByUsersAsync(int userId1, int userId2);
    Task<bool> UpdateStatusAsync(int friendshipId, FriendshipStatus status);
    Task<List<FriendDto>> GetFriendsAsync(int userId);
    Task<List<FriendRequestDto>> GetFriendRequestsAsync(int userId);
    Task<bool> RemoveFriendshipAsync(int userId, int friendId);
}
