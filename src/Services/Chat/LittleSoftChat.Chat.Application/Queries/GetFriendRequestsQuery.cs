using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Chat.Application.Queries;

public record GetFriendRequestsQuery(int UserId) : IRequest<List<FriendRequestDto>>;

public class GetFriendRequestsQueryValidator : AbstractValidator<GetFriendRequestsQuery>
{
    public GetFriendRequestsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

public class GetFriendRequestsQueryHandler : IRequestHandler<GetFriendRequestsQuery, List<FriendRequestDto>>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly ILogger<GetFriendRequestsQueryHandler> _logger;

    public GetFriendRequestsQueryHandler(
        IFriendshipRepository friendshipRepository,
        ILogger<GetFriendRequestsQueryHandler> logger)
    {
        _friendshipRepository = friendshipRepository;
        _logger = logger;
    }

    public async Task<List<FriendRequestDto>> Handle(GetFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _friendshipRepository.GetFriendRequestsAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friend requests for user {UserId}", request.UserId);
            return new List<FriendRequestDto>();
        }
    }
}
