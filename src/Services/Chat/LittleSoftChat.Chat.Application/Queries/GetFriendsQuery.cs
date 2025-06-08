using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Chat.Application.Queries;

public record GetFriendsQuery(int UserId) : IRequest<List<FriendDto>>;

public class GetFriendsQueryValidator : AbstractValidator<GetFriendsQuery>
{
    public GetFriendsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, List<FriendDto>>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ILogger<GetFriendsQueryHandler> _logger;

    public GetFriendsQueryHandler(
        IFriendshipRepository friendshipRepository,
        ILogger<GetFriendsQueryHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _logger = logger;
    }

    public async Task<List<FriendDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _friendshipRepository.GetFriendsAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friends for user {UserId}", request.UserId);
            return new List<FriendDto>();
        }
    }
}
