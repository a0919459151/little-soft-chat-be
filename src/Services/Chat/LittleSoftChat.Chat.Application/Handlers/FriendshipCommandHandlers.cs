using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Chat.Application.Commands;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.HttpClients;

namespace LittleSoftChat.Chat.Application.Handlers;

public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, FriendRequestResult>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserGrpcClient _userGrpcClient;
    private readonly INotificationHttpClient _notificationHttpClient;
    private readonly ILogger<SendFriendRequestCommandHandler> _logger;

    public SendFriendRequestCommandHandler(
        IFriendshipRepository friendshipRepository,
        IUserGrpcClient userGrpcClient,
        INotificationHttpClient notificationHttpClient,
        ILogger<SendFriendRequestCommandHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _userGrpcClient = userGrpcClient;
        _notificationHttpClient = notificationHttpClient;
        _logger = logger;
    }

    public async Task<FriendRequestResult> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate users
            var requester = await _userGrpcClient.GetUserAsync(request.RequesterId);
            var receiver = await _userGrpcClient.GetUserAsync(request.ReceiverId);

            if (requester == null || receiver == null || !requester.IsActive || !receiver.IsActive)
            {
                return FriendRequestResult.Failure("Invalid users");
            }

            // 2. Check if friendship already exists
            var existingFriendship = await _friendshipRepository.GetByUsersAsync(request.RequesterId, request.ReceiverId);
            if (existingFriendship != null)
            {
                return FriendRequestResult.Failure("Friend request already exists");
            }

            // 3. Create friendship request
            var friendship = new FriendshipEntity
            {
                UserId = request.RequesterId,
                FriendId = request.ReceiverId,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _friendshipRepository.CreateAsync(friendship);

            // 4. Send notification
            await _notificationHttpClient.SendNotificationAsync(
                request.ReceiverId, 
                $"Friend request from {requester.DisplayName}", 
                "friend_request"
            );

            _logger.LogInformation("Friend request sent from {RequesterId} to {ReceiverId}", 
                request.RequesterId, request.ReceiverId);

            return FriendRequestResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request from {RequesterId} to {ReceiverId}", 
                request.RequesterId, request.ReceiverId);
            return FriendRequestResult.Failure("Failed to send friend request");
        }
    }
}

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, FriendRequestResult>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly INotificationHttpClient _notificationHttpClient;
    private readonly ILogger<AcceptFriendRequestCommandHandler> _logger;

    public AcceptFriendRequestCommandHandler(
        IFriendshipRepository friendshipRepository,
        INotificationHttpClient notificationHttpClient,
        ILogger<AcceptFriendRequestCommandHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _notificationHttpClient = notificationHttpClient;
        _logger = logger;
    }

    public async Task<FriendRequestResult> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get friendship request
            var friendship = await _friendshipRepository.GetByIdAsync(request.FriendshipId);
            if (friendship == null || friendship.FriendId != request.UserId)
            {
                return FriendRequestResult.Failure("Friend request not found");
            }

            // 2. Update status to accepted
            var success = await _friendshipRepository.UpdateStatusAsync(request.FriendshipId, FriendshipStatus.Accepted);
            if (!success)
            {
                return FriendRequestResult.Failure("Failed to accept friend request");
            }

            // 3. Send notification
            await _notificationHttpClient.SendNotificationAsync(
                friendship.UserId, 
                "Your friend request was accepted", 
                "friend_accepted"
            );

            _logger.LogInformation("Friend request {FriendshipId} accepted by user {UserId}", 
                request.FriendshipId, request.UserId);

            return FriendRequestResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request {FriendshipId} by user {UserId}", 
                request.FriendshipId, request.UserId);
            return FriendRequestResult.Failure("Failed to accept friend request");
        }
    }
}

public class RejectFriendRequestCommandHandler : IRequestHandler<RejectFriendRequestCommand, FriendRequestResult>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ILogger<RejectFriendRequestCommandHandler> _logger;

    public RejectFriendRequestCommandHandler(
        IFriendshipRepository friendshipRepository,
        ILogger<RejectFriendRequestCommandHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _logger = logger;
    }

    public async Task<FriendRequestResult> Handle(RejectFriendRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get friendship request
            var friendship = await _friendshipRepository.GetByIdAsync(request.FriendshipId);
            if (friendship == null || friendship.FriendId != request.UserId)
            {
                return FriendRequestResult.Failure("Friend request not found");
            }

            // 2. Update status to rejected
            var success = await _friendshipRepository.UpdateStatusAsync(request.FriendshipId, FriendshipStatus.Rejected);

            if (success)
            {
                _logger.LogInformation("Friend request {FriendshipId} rejected by user {UserId}", 
                    request.FriendshipId, request.UserId);
            }

            return success ? FriendRequestResult.Success() : FriendRequestResult.Failure("Failed to reject friend request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request {FriendshipId} by user {UserId}", 
                request.FriendshipId, request.UserId);
            return FriendRequestResult.Failure("Failed to reject friend request");
        }
    }
}

public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, bool>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ILogger<RemoveFriendCommandHandler> _logger;

    public RemoveFriendCommandHandler(
        IFriendshipRepository friendshipRepository,
        ILogger<RemoveFriendCommandHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _friendshipRepository.RemoveFriendshipAsync(request.UserId, request.FriendId);
            
            if (success)
            {
                _logger.LogInformation("Friendship removed between users {UserId} and {FriendId}", 
                    request.UserId, request.FriendId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friendship between users {UserId} and {FriendId}", 
                request.UserId, request.FriendId);
            return false;
        }
    }
}
