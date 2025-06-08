using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LittleSoftChat.Gateway.API.Models;
using LittleSoftChat.Gateway.API.Services;

namespace LittleSoftChat.Gateway.API.Controllers;

/// <summary>
/// 聊天相關 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// 發送訊息
    /// </summary>
    /// <param name="request">發送訊息請求</param>
    /// <returns>發送結果</returns>
    /// <response code="200">發送成功</response>
    /// <response code="400">發送失敗</response>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(SendMessageResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.SendMessageAsync(userId, request);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// 取得與好友的聊天記錄
    /// </summary>
    /// <param name="friendId">好友 ID</param>
    /// <param name="page">頁碼</param>
    /// <param name="size">每頁筆數</param>
    /// <returns>聊天記錄</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("messages/{friendId}")]
    [ProducesResponseType(typeof(GetMessagesResponse), 200)]
    public async Task<IActionResult> GetMessages(int friendId, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.GetMessagesAsync(userId, friendId, page, size);
        
        return Ok(result);
    }

    /// <summary>
    /// 標記訊息為已讀
    /// </summary>
    /// <param name="messageId">訊息 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    [HttpPut("messages/{messageId}/read")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkMessageAsRead(int messageId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);
        
        return Ok(new { success = result });
    }
}
