using MediatR;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Chat.Application.Commands;

// Message Commands
public record SendMessageCommand(int SenderId, int ReceiverId, string Content, string MessageType = "text") : IRequest<SendMessageResult>;

public record MarkMessageAsReadCommand(int MessageId, int UserId) : IRequest<bool>;

// Friend Request Commands
public record SendFriendRequestCommand(int RequesterId, int ReceiverId) : IRequest<FriendRequestResult>;

public record AcceptFriendRequestCommand(int FriendshipId, int UserId) : IRequest<FriendRequestResult>;

public record RejectFriendRequestCommand(int FriendshipId, int UserId) : IRequest<FriendRequestResult>;

public record RemoveFriendCommand(int UserId, int FriendId) : IRequest<bool>;
