using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.HttpClients;

namespace LittleSoftChat.Chat.Application.Commands;

public record SendFriendRequestCommand(int RequesterId, int ReceiverId) : IRequest<FriendRequestResult>;

public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.RequesterId)
            .GreaterThan(0)
            .WithMessage("RequesterId must be greater than 0");

        RuleFor(x => x.ReceiverId)
            .GreaterThan(0)
            .WithMessage("ReceiverId must be greater than 0");

        RuleFor(x => x)
            .Must(command => command.RequesterId != command.ReceiverId)
            .WithMessage("Cannot send friend request to yourself");
    }
}

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
