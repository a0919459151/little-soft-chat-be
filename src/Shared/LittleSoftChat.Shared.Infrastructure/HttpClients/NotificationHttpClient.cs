using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LittleSoftChat.Shared.Infrastructure.HttpClients;

public interface INotificationHttpClient
{
    Task<bool> SendNotificationAsync(int userId, string message, string type);
    Task<bool> SendBulkNotificationAsync(IEnumerable<int> userIds, string message, string type);
}

public class NotificationHttpClient : INotificationHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationHttpClient> _logger;

    public NotificationHttpClient(HttpClient httpClient, ILogger<NotificationHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendNotificationAsync(int userId, string message, string type)
    {
        try
        {
            var request = new
            {
                UserId = userId,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/notifications", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Notification sent successfully to user {UserId}", userId);
                return true;
            }
            
            _logger.LogWarning("Failed to send notification to user {UserId}. Status: {StatusCode}", 
                userId, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when sending notification to user {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when sending notification to user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendBulkNotificationAsync(IEnumerable<int> userIds, string message, string type)
    {
        try
        {
            var request = new
            {
                UserIds = userIds,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/notifications/bulk", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications");
            return false;
        }
    }
}
