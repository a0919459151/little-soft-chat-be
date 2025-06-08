using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LittleSoftChat.Gateway.Presentation.Models;
using LittleSoftChat.Gateway.Presentation.Services;

namespace LittleSoftChat.Gateway.Presentation.Controllers;

/// <summary>
/// 好友相關 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FriendsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(IChatService chatService, ILogger<FriendsController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// 取得好友列表
    /// </summary>
    /// <returns>好友列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FriendResponse>), 200)]
    public async Task<IActionResult> GetFriends()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.GetFriendsAsync(userId);
        
        return Ok(result);
    }

    /// <summary>
    /// 取得好友請求列表
    /// </summary>
    /// <returns>好友請求列表</returns>
    /// <response code="200">取得成功</response>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(List<FriendRequestResponse>), 200)]
    public async Task<IActionResult> GetFriendRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.GetFriendRequestsAsync(userId);
        
        return Ok(result);
    }

    /// <summary>
    /// 發送好友請求
    /// </summary>
    /// <param name="request">好友請求</param>
    /// <returns>發送結果</returns>
    /// <response code="200">發送成功</response>
    /// <response code="400">發送失敗</response>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(FriendRequestResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.SendFriendRequestAsync(userId, request.ReceiverId);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// 接受好友請求
    /// </summary>
    /// <param name="requestId">好友請求 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    /// <response code="400">操作失敗</response>
    [HttpPost("requests/{requestId}/accept")]
    [ProducesResponseType(typeof(FriendRequestResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.AcceptFriendRequestAsync(requestId, userId);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// 拒絕好友請求
    /// </summary>
    /// <param name="requestId">好友請求 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    /// <response code="400">操作失敗</response>
    [HttpPost("requests/{requestId}/reject")]
    [ProducesResponseType(typeof(FriendRequestResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RejectFriendRequest(int requestId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.RejectFriendRequestAsync(requestId, userId);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// 移除好友
    /// </summary>
    /// <param name="friendId">好友 ID</param>
    /// <returns>操作結果</returns>
    /// <response code="200">操作成功</response>
    [HttpDelete("{friendId}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _chatService.RemoveFriendAsync(userId, friendId);
        
        return Ok(new { success = result });
    }
}
