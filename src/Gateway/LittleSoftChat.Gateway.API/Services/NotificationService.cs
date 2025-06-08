using GatewayModels = LittleSoftChat.Gateway.API.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace LittleSoftChat.Gateway.API.Services;

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
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GatewayModels.GetNotificationsResponse> GetNotificationsAsync(int userId, int page, int size)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/notifications?page={page}&size={size}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GatewayModels.GetNotificationsResponse>(content);
                return result ?? new GatewayModels.GetNotificationsResponse
                {
                    Notifications = new List<GatewayModels.NotificationResponse>(),
                    TotalCount = 0,
                    Page = page,
                    Size = size
                };
            }

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
            _logger.LogError(ex, "Error getting notifications");
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
            var response = await _httpClient.GetAsync($"api/notifications/unread-count");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (int.TryParse(content, out int count))
                {
                    return count;
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return 0;
        }
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/notifications/{notificationId}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return false;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        try
        {
            var response = await _httpClient.PutAsync("api/notifications/mark-all-read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return false;
        }
    }

    public async Task<bool> SendNotificationAsync(GatewayModels.SendNotificationRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/notifications", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return false;
        }
    }
}
