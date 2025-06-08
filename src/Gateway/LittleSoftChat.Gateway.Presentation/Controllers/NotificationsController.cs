using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LittleSoftChat.Gateway.Presentation.Models;
using LittleSoftChat.Gateway.Presentation.Services;

namespace LittleSoftChat.Gateway.Presentation.Controllers;

/// <summary>
/// 通知相關 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// 取得通知列表
    /// </summary>
    /// <param name="page">頁碼</param>
    /// <param name="size">每頁筆數</param>
    /// <returns>通知列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetNotificationsResponse), 200)]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _notificationService.GetNotificationsAsync(userId, page, size);
        
        return Ok(result);
    }

    /// <summary>
    /// 取得未讀通知數量
    /// </summary>
    /// <returns>未讀通知數量</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _notificationService.GetUnreadCountAsync(userId);
        
        return Ok(new { count = result });
    }

    /// <summary>
    /// 標記通知為已讀
    /// </summary>
    /// <param name="notificationId">通知 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    [HttpPost("{notificationId}/read")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
        
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// 標記所有通知為已讀
    /// </summary>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    [HttpPost("read-all")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _notificationService.MarkAllAsReadAsync(userId);
        
        return result ? Ok() : BadRequest();
    }
}
