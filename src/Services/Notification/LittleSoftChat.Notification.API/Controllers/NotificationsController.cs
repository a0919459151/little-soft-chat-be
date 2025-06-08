using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using LittleSoftChat.Notification.Application.Commands;
using LittleSoftChat.Notification.Application.Queries;

namespace LittleSoftChat.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetNotificationHistoryQuery(userId, page, size);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetUnreadNotificationCountQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(new { count = result });
    }

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new MarkNotificationAsReadCommand(notificationId, userId);
        var result = await _mediator.Send(command);
        
        return result ? Ok() : BadRequest();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await _mediator.Send(command);
        
        return result ? Ok() : BadRequest();
    }

    [HttpGet("online-status/{userId}")]
    public async Task<IActionResult> GetUserOnlineStatus(int userId)
    {
        var query = new GetUserOnlineStatusQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(new { userId, isOnline = result });
    }

    [HttpPost("send")]
    [AllowAnonymous] // 允許其他服務調用
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        var command = new SendRealtimeNotificationCommand(
            request.UserId,
            request.Type,
            request.Title,
            request.Content,
            request.Data
        );
        
        var result = await _mediator.Send(command);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("system")]
    [AllowAnonymous] // 允許其他服務調用
    public async Task<IActionResult> SendSystemNotification([FromBody] SendSystemNotificationRequest request)
    {
        var command = new SendSystemNotificationCommand(request.UserIds, request.Title, request.Content);
        var result = await _mediator.Send(command);
        
        return result ? Ok() : BadRequest();
    }
}

public class SendNotificationRequest
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class SendSystemNotificationRequest
{
    public List<int> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
