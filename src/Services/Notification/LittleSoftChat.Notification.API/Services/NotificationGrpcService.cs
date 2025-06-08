using Grpc.Core;
using MediatR;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Notification.Application.Commands;
using LittleSoftChat.Notification.Application.Queries;
using Google.Protobuf.WellKnownTypes;

namespace LittleSoftChat.Notification.API.Services;

public class NotificationGrpcService : NotificationService.NotificationServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationGrpcService> _logger;

    public NotificationGrpcService(IMediator mediator, ILogger<NotificationGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<NotificationResponse> SendNotification(SendNotificationRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SendRealtimeNotificationCommand(
                request.UserId, 
                request.Title, 
                request.Content, 
                request.NotificationType);

            var result = await _mediator.Send(command);

            return new NotificationResponse
            {
                Success = result,
                ErrorMessage = result ? "" : "Failed to send notification"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendNotification gRPC call");
            return new NotificationResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<GetNotificationsResponse> GetNotifications(GetNotificationsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetNotificationHistoryQuery(request.UserId, request.Page, request.PageSize);
            var result = await _mediator.Send(query);

            var response = new GetNotificationsResponse();
            foreach (var notification in result.Items)
            {
                response.Notifications.Add(new NotificationInfo
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Content = notification.Content,
                    NotificationType = notification.Type,
                    IsRead = notification.IsRead,
                    CreatedAt = Timestamp.FromDateTime(notification.CreatedAt.ToUniversalTime())
                });
            }
            response.TotalCount = result.TotalCount;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNotifications gRPC call");
            return new GetNotificationsResponse();
        }
    }

    public override async Task<NotificationResponse> MarkAsRead(MarkAsReadRequest request, ServerCallContext context)
    {
        try
        {
            var command = new MarkNotificationAsReadCommand(request.NotificationId, request.UserId);
            var result = await _mediator.Send(command);

            return new NotificationResponse
            {
                Success = result,
                ErrorMessage = result ? "" : "Failed to mark notification as read"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MarkAsRead gRPC call");
            return new NotificationResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<UnreadCountResponse> GetUnreadCount(GetUnreadCountRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetUnreadNotificationCountQuery(request.UserId);
            var count = await _mediator.Send(query);

            return new UnreadCountResponse
            {
                Count = count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUnreadCount gRPC call");
            return new UnreadCountResponse { Count = 0 };
        }
    }

    public override async Task<NotificationResponse> DeleteNotification(DeleteNotificationRequest request, ServerCallContext context)
    {
        try
        {
            // For now, we can implement a simple deletion command if needed
            // This would require adding a DeleteNotificationCommand to the application layer
            await Task.CompletedTask; // Remove this when implementing actual deletion logic
            
            return new NotificationResponse
            {
                Success = true,
                ErrorMessage = ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteNotification gRPC call");
            return new NotificationResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<BroadcastResponse> BroadcastMessage(BroadcastMessageRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SendSystemNotificationCommand(
                request.TargetUserIds.ToList(),
                request.Title, 
                request.Content);

            var result = await _mediator.Send(command);

            return new BroadcastResponse
            {
                Success = result,
                SentCount = request.TargetUserIds.Count,
                ErrorMessage = result ? "" : "Failed to send broadcast"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BroadcastMessage gRPC call");
            return new BroadcastResponse
            {
                Success = false,
                SentCount = 0,
                ErrorMessage = "Internal server error"
            };
        }
    }
}
