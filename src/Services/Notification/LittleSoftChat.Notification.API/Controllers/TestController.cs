using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LittleSoftChat.Notification.Application.Commands;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IMediator mediator,
        INotificationService notificationService,
        ILogger<TestController> logger)
    {
        _mediator = mediator;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("send-test-notification")]
    public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        try
        {
            var command = new SendRealtimeNotificationCommand(
                request.UserId,
                "test",
                request.Title,
                request.Content,
                new { TestData = "This is a test notification" }
            );

            var result = await _mediator.Send(command);
            
            return Ok(new { Success = result, Message = "Test notification sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test notification");
            return StatusCode(500, new { Success = false, Message = "Failed to send notification" });
        }
    }

    [HttpPost("send-system-broadcast")]
    public async Task<IActionResult> SendSystemBroadcast([FromBody] SystemBroadcastRequest request)
    {
        try
        {
            var command = new SendSystemNotificationCommand(
                request.UserIds,
                request.Title,
                request.Content
            );

            var result = await _mediator.Send(command);
            
            return Ok(new { Success = result, Message = "System broadcast sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system broadcast");
            return StatusCode(500, new { Success = false, Message = "Failed to send broadcast" });
        }
    }

    [HttpGet("online-status/{userId}")]
    public async Task<IActionResult> CheckOnlineStatus(int userId)
    {
        try
        {
            var isOnline = await _notificationService.IsUserOnlineAsync(userId);
            return Ok(new { UserId = userId, IsOnline = isOnline });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check online status for user {UserId}", userId);
            return StatusCode(500, new { Success = false, Message = "Failed to check online status" });
        }
    }
}

public class TestNotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SystemBroadcastRequest
{
    public List<int> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
