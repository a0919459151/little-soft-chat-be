using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;

namespace LittleSoftChat.Chat.Application.Commands;

public record RemoveFriendCommand(int UserId, int FriendId) : IRequest<bool>;

public class RemoveFriendCommandValidator : AbstractValidator<RemoveFriendCommand>
{
    public RemoveFriendCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.FriendId)
            .GreaterThan(0)
            .WithMessage("FriendId must be greater than 0");

        RuleFor(x => x)
            .Must(command => command.UserId != command.FriendId)
            .WithMessage("Cannot remove yourself as friend");
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
