using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Chat.Application.Queries;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Chat.Application.Handlers;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<GetMessagesQueryHandler> _logger;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        ILogger<GetMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _messageRepository.GetMessagesAsync(request.UserId, request.FriendId, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for users {UserId} and {FriendId}", 
                request.UserId, request.FriendId);
            return new PagedResult<MessageDto>(new List<MessageDto>(), request.Page, request.Size, 0);
        }
    }
}

public class GetUnreadMessagesQueryHandler : IRequestHandler<GetUnreadMessagesQuery, List<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<GetUnreadMessagesQueryHandler> _logger;

    public GetUnreadMessagesQueryHandler(
        IMessageRepository messageRepository,
        ILogger<GetUnreadMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<List<MessageDto>> Handle(GetUnreadMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _messageRepository.GetUnreadMessagesAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages for user {UserId}", request.UserId);
            return new List<MessageDto>();
        }
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
