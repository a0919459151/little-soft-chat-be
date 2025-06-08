using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;

namespace LittleSoftChat.Chat.Application.Commands;

public record RejectFriendRequestCommand(int FriendshipId, int UserId) : IRequest<FriendRequestResult>;

public class RejectFriendRequestCommandValidator : AbstractValidator<RejectFriendRequestCommand>
{
    public RejectFriendRequestCommandValidator()
    {
        RuleFor(x => x.FriendshipId)
            .GreaterThan(0)
            .WithMessage("FriendshipId must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
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
