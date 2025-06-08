using MediatR;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Common;

namespace LittleSoftChat.Notification.Application.Queries;

// 獲取用戶通知歷史
public record GetNotificationHistoryQuery(
    int UserId,
    int Page = 1,
    int Size = 20
) : IRequest<PagedResult<NotificationDto>>;

// 獲取未讀通知數量
public record GetUnreadNotificationCountQuery(
    int UserId
) : IRequest<int>;

// 檢查用戶是否在線
public record GetUserOnlineStatusQuery(
    int UserId
) : IRequest<bool>;

// 獲取多個用戶在線狀態
public record GetUsersOnlineStatusQuery(
    List<int> UserIds
) : IRequest<Dictionary<int, bool>>;
