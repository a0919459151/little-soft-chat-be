using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Shared.Domain.Results;
using Google.Protobuf.WellKnownTypes;
using GatewayModels = LittleSoftChat.Gateway.Presentation.Models;

namespace LittleSoftChat.Gateway.Presentation.Services;

public interface IChatService
{
    Task<GatewayModels.SendMessageResponse> SendMessageAsync(int senderId, GatewayModels.SendMessageRequest request);
    Task<GatewayModels.GetMessagesResponse> GetMessagesAsync(int userId, int friendId, int page, int size);
    Task<List<GatewayModels.FriendResponse>> GetFriendsAsync(int userId);
    Task<List<GatewayModels.FriendRequestResponse>> GetFriendRequestsAsync(int userId);
    Task<FriendRequestResult> SendFriendRequestAsync(int senderId, int receiverId);
    Task<FriendRequestResult> AcceptFriendRequestAsync(int requestId, int userId);
    Task<FriendRequestResult> RejectFriendRequestAsync(int requestId, int userId);
    Task<bool> RemoveFriendAsync(int userId, int friendId);
    Task<bool> MarkMessageAsReadAsync(int messageId, int userId);
}

public class ChatService : IChatService
{
    private readonly LittleSoftChat.Shared.Contracts.ChatService.ChatServiceClient _chatClient;
    private readonly ILogger<ChatService> _logger;

    public ChatService(LittleSoftChat.Shared.Contracts.ChatService.ChatServiceClient chatClient, ILogger<ChatService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<GatewayModels.SendMessageResponse> SendMessageAsync(int senderId, GatewayModels.SendMessageRequest request)
    {
        try
        {
            var grpcRequest = new LittleSoftChat.Shared.Contracts.SendMessageRequest
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType
            };

            var response = await _chatClient.SendMessageAsync(grpcRequest);

            return new GatewayModels.SendMessageResponse
            {
                IsSuccess = response.Success,
                Message = response.ErrorMessage,
                MessageId = response.MessageId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return new GatewayModels.SendMessageResponse
            {
                IsSuccess = false,
                Message = "Failed to send message"
            };
        }
    }

    public async Task<GatewayModels.GetMessagesResponse> GetMessagesAsync(int userId, int friendId, int page, int size)
    {
        try
        {
            var request = new LittleSoftChat.Shared.Contracts.GetMessagesRequest
            {
                UserId = userId,
                FriendId = friendId,
                Page = page,
                PageSize = size
            };

            var response = await _chatClient.GetMessagesAsync(request);

            return new GatewayModels.GetMessagesResponse
            {
                Messages = response.Messages.Select(m => new GatewayModels.MessageResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    SentAt = m.SentAt.ToDateTime(),
                    IsRead = m.IsRead
                }).ToList(),
                TotalCount = response.TotalCount,
                Page = page,
                Size = size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return new GatewayModels.GetMessagesResponse
            {
                Messages = new List<GatewayModels.MessageResponse>(),
                TotalCount = 0,
                Page = page,
                Size = size
            };
        }
    }

    public async Task<List<GatewayModels.FriendResponse>> GetFriendsAsync(int userId)
    {
        try
        {
            var request = new GetFriendsRequest { UserId = userId };
            var response = await _chatClient.GetFriendsAsync(request);

            return response.Friends.Select(f => new GatewayModels.FriendResponse
            {
                Id = f.Id,
                Username = f.Username,
                FullName = f.DisplayName,
                IsOnline = f.IsOnline,
                LastSeen = f.LastSeen?.ToDateTime()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friends");
            return new List<GatewayModels.FriendResponse>();
        }
    }

    public async Task<List<GatewayModels.FriendRequestResponse>> GetFriendRequestsAsync(int userId)
    {
        try
        {
            var request = new GetFriendRequestsRequest { UserId = userId };
            var response = await _chatClient.GetFriendRequestsAsync(request);

            return response.Requests.Select(r => new GatewayModels.FriendRequestResponse
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderUsername = r.SenderUsername,
                SenderFullName = r.SenderDisplayName,
                SentAt = r.CreatedAt.ToDateTime(),
                Status = "Pending" // Default status for friend requests
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friend requests");
            return new List<GatewayModels.FriendRequestResponse>();
        }
    }

    public async Task<FriendRequestResult> SendFriendRequestAsync(int senderId, int receiverId)
    {
        try
        {
            var request = new LittleSoftChat.Shared.Contracts.SendFriendRequestRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = "" // Default empty message
            };

            var response = await _chatClient.SendFriendRequestAsync(request);
            
            return response.Success 
                ? FriendRequestResult.Success() 
                : FriendRequestResult.Failure(response.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request");
            return FriendRequestResult.Failure("Failed to send friend request");
        }
    }

    public async Task<FriendRequestResult> AcceptFriendRequestAsync(int requestId, int userId)
    {
        try
        {
            var request = new AcceptFriendRequestRequest
            {
                RequestId = requestId,
                UserId = userId
            };

            var response = await _chatClient.AcceptFriendRequestAsync(request);
            
            return response.Success 
                ? FriendRequestResult.Success() 
                : FriendRequestResult.Failure(response.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request");
            return FriendRequestResult.Failure("Failed to accept friend request");
        }
    }

    public async Task<FriendRequestResult> RejectFriendRequestAsync(int requestId, int userId)
    {
        try
        {
            var request = new RejectFriendRequestRequest
            {
                RequestId = requestId,
                UserId = userId
            };

            var response = await _chatClient.RejectFriendRequestAsync(request);
            
            return response.Success 
                ? FriendRequestResult.Success() 
                : FriendRequestResult.Failure(response.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request");
            return FriendRequestResult.Failure("Failed to reject friend request");
        }
    }

    public async Task<bool> RemoveFriendAsync(int userId, int friendId)
    {
        try
        {
            var request = new RemoveFriendRequest
            {
                UserId = userId,
                FriendId = friendId
            };

            var response = await _chatClient.RemoveFriendAsync(request);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend");
            return false;
        }
    }

    public Task<bool> MarkMessageAsReadAsync(int messageId, int userId)
    {
        try
        {
            // Note: This method may need to be implemented based on your actual gRPC contract
            // For now, returning true as a placeholder
            _logger.LogWarning("MarkMessageAsReadAsync not implemented in gRPC contract");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            return Task.FromResult(false);
        }
    }
}
