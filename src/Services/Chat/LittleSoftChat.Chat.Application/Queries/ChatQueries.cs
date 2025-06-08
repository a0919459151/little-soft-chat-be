using MediatR;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Chat.Application.Queries;

// Message Queries
public record GetMessagesQuery(int UserId, int FriendId, int Page = 1, int Size = 20) : IRequest<PagedResult<MessageDto>>;

public record GetUnreadMessagesQuery(int UserId) : IRequest<List<MessageDto>>;

// Friend Queries  
public record GetFriendsQuery(int UserId) : IRequest<List<FriendDto>>;

public record GetFriendRequestsQuery(int UserId) : IRequest<List<FriendRequestDto>>;
