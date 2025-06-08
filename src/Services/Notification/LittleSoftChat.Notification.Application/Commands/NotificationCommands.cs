using MediatR;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Notification.Application.Commands;

// 發送即時通知
public record SendRealtimeNotificationCommand(
    int UserId,
    string Type,
    string Title,
    string Content,
    object? Data = null
) : IRequest<bool>;

// 發送系統通知  
public record SendSystemNotificationCommand(
    List<int> UserIds,
    string Title,
    string Content
) : IRequest<bool>;

// 標記通知為已讀
public record MarkNotificationAsReadCommand(
    int NotificationId,
    int UserId
) : IRequest<bool>;

// 標記所有通知為已讀
public record MarkAllNotificationsAsReadCommand(
    int UserId
) : IRequest<bool>;
