using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.HttpClients;

namespace LittleSoftChat.Chat.Application.Commands;

public record AcceptFriendRequestCommand(int FriendshipId, int UserId) : IRequest<FriendRequestResult>;

public class AcceptFriendRequestCommandValidator : AbstractValidator<AcceptFriendRequestCommand>
{
    public AcceptFriendRequestCommandValidator()
    {
        RuleFor(x => x.FriendshipId)
            .GreaterThan(0)
            .WithMessage("FriendshipId must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
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
