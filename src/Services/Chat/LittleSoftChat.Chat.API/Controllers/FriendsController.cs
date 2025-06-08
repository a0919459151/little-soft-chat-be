using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using LittleSoftChat.Chat.Application.Commands;
using LittleSoftChat.Chat.Application.Queries;
using LittleSoftChat.Chat.API.Models;

namespace LittleSoftChat.Chat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(IMediator mediator, ILogger<FriendsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetFriendsQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetFriendRequestsQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new SendFriendRequestCommand(userId, request.ReceiverId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("requests/{friendshipId}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(int friendshipId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new AcceptFriendRequestCommand(friendshipId, userId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("requests/{friendshipId}/reject")]
    public async Task<IActionResult> RejectFriendRequest(int friendshipId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new RejectFriendRequestCommand(friendshipId, userId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{friendId}")]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new RemoveFriendCommand(userId, friendId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
}
