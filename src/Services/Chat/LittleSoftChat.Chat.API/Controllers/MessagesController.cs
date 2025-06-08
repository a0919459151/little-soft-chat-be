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
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMediator mediator, ILogger<MessagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new SendMessageCommand(userId, request.ReceiverId, request.Content, request.MessageType);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{friendId}")]
    public async Task<IActionResult> GetMessages(int friendId, [FromQuery] GetMessagesRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetMessagesQuery(userId, friendId, request.Page, request.Size);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpPut("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new MarkMessageAsReadCommand(messageId, userId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadMessages()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetUnreadMessagesQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
