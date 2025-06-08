using Grpc.Core;
using MediatR;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Chat.Application.Commands;
using LittleSoftChat.Chat.Application.Queries;
using Google.Protobuf.WellKnownTypes;

namespace LittleSoftChat.Chat.Presentation.Services;

public class ChatGrpcService : ChatService.ChatServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatGrpcService> _logger;

    public ChatGrpcService(IMediator mediator, ILogger<ChatGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SendMessageCommand(
                request.SenderId, 
                request.ReceiverId, 
                request.Content, 
                request.MessageType);

            var result = await _mediator.Send(command);

            return new SendMessageResponse
            {
                Success = result.IsSuccess,
                MessageId = result.MessageId,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage gRPC call");
            return new SendMessageResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<GetMessagesResponse> GetMessages(GetMessagesRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetMessagesQuery(request.UserId, request.FriendId, request.Page, request.PageSize);
            var result = await _mediator.Send(query);

            var response = new GetMessagesResponse
            {
                TotalCount = result.TotalCount
            };

            foreach (var message in result.Items)
            {
                response.Messages.Add(new MessageResponse
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Content = message.Content,
                    MessageType = message.MessageType,
                    IsRead = message.IsRead,
                    SentAt = Timestamp.FromDateTime(message.CreatedAt.ToUniversalTime())
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMessages gRPC call");
            return new GetMessagesResponse();
        }
    }

    public override async Task<FriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SendFriendRequestCommand(request.SenderId, request.ReceiverId);
            var result = await _mediator.Send(command);

            return new FriendRequestResponse
            {
                Success = result.IsSuccess,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendFriendRequest gRPC call");
            return new FriendRequestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<FriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
    {
        try
        {
            var command = new AcceptFriendRequestCommand(request.RequestId, request.UserId);
            var result = await _mediator.Send(command);

            return new FriendRequestResponse
            {
                Success = result.IsSuccess,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AcceptFriendRequest gRPC call");
            return new FriendRequestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<FriendRequestResponse> RejectFriendRequest(RejectFriendRequestRequest request, ServerCallContext context)
    {
        try
        {
            var command = new RejectFriendRequestCommand(request.RequestId, request.UserId);
            var result = await _mediator.Send(command);

            return new FriendRequestResponse
            {
                Success = result.IsSuccess,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RejectFriendRequest gRPC call");
            return new FriendRequestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<GetFriendsResponse> GetFriends(GetFriendsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetFriendsQuery(request.UserId);
            var result = await _mediator.Send(query);

            var response = new GetFriendsResponse();

            foreach (var friend in result)
            {
                response.Friends.Add(new FriendResponse
                {
                    Id = friend.Friend.Id,
                    Username = friend.Friend.Username,
                    DisplayName = friend.Friend.DisplayName,
                    Avatar = friend.Friend.Avatar,
                    IsOnline = friend.Friend.IsActive,
                    LastSeen = Timestamp.FromDateTime(friend.CreatedAt.ToUniversalTime())
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFriends gRPC call");
            return new GetFriendsResponse();
        }
    }

    public override async Task<GetFriendRequestsResponse> GetFriendRequests(GetFriendRequestsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetFriendRequestsQuery(request.UserId);
            var result = await _mediator.Send(query);

            var response = new GetFriendRequestsResponse();

            foreach (var friendRequest in result)
            {
                response.Requests.Add(new FriendRequestInfo
                {
                    Id = friendRequest.Id,
                    SenderId = friendRequest.RequesterId,
                    SenderUsername = friendRequest.Requester.Username,
                    SenderDisplayName = friendRequest.Requester.DisplayName,
                    SenderAvatar = friendRequest.Requester.Avatar,
                    Message = "", // Add default empty message as it's not in our DTO
                    CreatedAt = Timestamp.FromDateTime(friendRequest.CreatedAt.ToUniversalTime())
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFriendRequests gRPC call");
            return new GetFriendRequestsResponse();
        }
    }

    public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
    {
        try
        {
            var command = new RemoveFriendCommand(request.UserId, request.FriendId);
            var result = await _mediator.Send(command);

            return new RemoveFriendResponse
            {
                Success = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveFriend gRPC call");
            return new RemoveFriendResponse
            {
                Success = false
            };
        }
    }
}
