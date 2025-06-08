using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Notification.Application.Queries;
using LittleSoftChat.Notification.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Notification.Application.Handlers;

public class GetNotificationHistoryQueryHandler : IRequestHandler<GetNotificationHistoryQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationHistoryRepository _historyRepository;
    private readonly ILogger<GetNotificationHistoryQueryHandler> _logger;

    public GetNotificationHistoryQueryHandler(
        INotificationHistoryRepository historyRepository,
        ILogger<GetNotificationHistoryQueryHandler> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notifications = await _historyRepository.GetByUserIdAsync(request.UserId, request.Page, request.Size);
            var totalCount = notifications.Count; // 簡化實現，實際應該有總數查詢

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            }).ToList();

            return new PagedResult<NotificationDto>(notificationDtos, request.Page, request.Size, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification history for user {UserId}", request.UserId);
            return new PagedResult<NotificationDto>();
        }
    }
}

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly INotificationHistoryRepository _historyRepository;
    private readonly ILogger<GetUnreadNotificationCountQueryHandler> _logger;

    public GetUnreadNotificationCountQueryHandler(
        INotificationHistoryRepository historyRepository,
        ILogger<GetUnreadNotificationCountQueryHandler> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _historyRepository.GetUnreadCountAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread notification count for user {UserId}", request.UserId);
            return 0;
        }
    }
}

public class GetUserOnlineStatusQueryHandler : IRequestHandler<GetUserOnlineStatusQuery, bool>
{
    private readonly INotificationConnectionRepository _connectionRepository;
    private readonly ILogger<GetUserOnlineStatusQueryHandler> _logger;

    public GetUserOnlineStatusQueryHandler(
        INotificationConnectionRepository connectionRepository,
        ILogger<GetUserOnlineStatusQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(GetUserOnlineStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _connectionRepository.IsUserOnlineAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online status for user {UserId}", request.UserId);
            return false;
        }
    }
}

public class GetUsersOnlineStatusQueryHandler : IRequestHandler<GetUsersOnlineStatusQuery, Dictionary<int, bool>>
{
    private readonly INotificationConnectionRepository _connectionRepository;
    private readonly ILogger<GetUsersOnlineStatusQueryHandler> _logger;

    public GetUsersOnlineStatusQueryHandler(
        INotificationConnectionRepository connectionRepository,
        ILogger<GetUsersOnlineStatusQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<Dictionary<int, bool>> Handle(GetUsersOnlineStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = new Dictionary<int, bool>();
            
            foreach (var userId in request.UserIds)
            {
                var isOnline = await _connectionRepository.IsUserOnlineAsync(userId);
                result[userId] = isOnline;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online status for multiple users");
            return new Dictionary<int, bool>();
        }
    }
}
