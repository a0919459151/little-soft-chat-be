using GatewayModels = LittleSoftChat.Gateway.Presentation.Models;
using LittleSoftChat.Shared.Contracts;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace LittleSoftChat.Gateway.Presentation.Services;

public interface INotificationService
{
    Task<GatewayModels.GetNotificationsResponse> GetNotificationsAsync(int userId, int page, int size);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<bool> SendNotificationAsync(GatewayModels.SendNotificationRequest request);
}

public class NotificationService : INotificationService
{
    private readonly LittleSoftChat.Shared.Contracts.NotificationService.NotificationServiceClient _grpcClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        LittleSoftChat.Shared.Contracts.NotificationService.NotificationServiceClient grpcClient, 
        ILogger<NotificationService> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<GatewayModels.GetNotificationsResponse> GetNotificationsAsync(int userId, int page, int size)
    {
        try
        {
            var request = new GetNotificationsRequest
            {
                UserId = userId,
                Page = page,
                PageSize = size
            };

            var response = await _grpcClient.GetNotificationsAsync(request);

            return new GatewayModels.GetNotificationsResponse
            {
                Notifications = response.Notifications.Select(n => new GatewayModels.NotificationResponse
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Type = n.NotificationType,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt.ToDateTime()
                }).ToList(),
                TotalCount = response.TotalCount,
                Page = page,
                Size = size
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting notifications for user {UserId}", userId);
            return new GatewayModels.GetNotificationsResponse
            {
                Notifications = new List<GatewayModels.NotificationResponse>(),
                TotalCount = 0,
                Page = page,
                Size = size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return new GatewayModels.GetNotificationsResponse
            {
                Notifications = new List<GatewayModels.NotificationResponse>(),
                TotalCount = 0,
                Page = page,
                Size = size
            };
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            var request = new GetUnreadCountRequest { UserId = userId };
            var response = await _grpcClient.GetUnreadCountAsync(request);
            return response.Count;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting unread notification count for user {UserId}", userId);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var request = new MarkAsReadRequest 
            { 
                NotificationId = notificationId,
                UserId = userId
            };
            var response = await _grpcClient.MarkAsReadAsync(request);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
            return false;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        try
        {
            var request = new MarkAllAsReadRequest { UserId = userId };
            var response = await _grpcClient.MarkAllAsReadAsync(request);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error marking all notifications as read for user {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendNotificationAsync(GatewayModels.SendNotificationRequest request)
    {
        try
        {
            var grpcRequest = new SendNotificationRequest
            {
                UserId = request.UserId,
                Title = request.Title,
                Content = request.Content,
                NotificationType = request.Type
            };

            var response = await _grpcClient.SendNotificationAsync(grpcRequest);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error sending notification to user {UserId}", request.UserId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", request.UserId);
            return false;
        }
    }
}
