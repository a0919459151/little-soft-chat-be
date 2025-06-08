using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using MediatR;
using LittleSoftChat.Notification.Application.Commands;
using LittleSoftChat.Notification.Application.Queries;
using LittleSoftChat.Notification.Domain.Repositories;

namespace LittleSoftChat.Notification.Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly INotificationConnectionRepository _connectionRepository;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IMediator mediator, 
        INotificationConnectionRepository connectionRepository,
        ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            // 添加到用戶組
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // 記錄連接
            await _connectionRepository.AddConnectionAsync(Context.ConnectionId, userId);
            
            _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            // 從用戶組移除
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // 移除連接記錄
            await _connectionRepository.RemoveConnectionAsync(Context.ConnectionId);
            
            _logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    // 客戶端可以調用的方法
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("ConnectionId {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("ConnectionId {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    // 發送私人訊息通知
    public async Task SendPrivateMessageNotification(int receiverId, string message)
    {
        var senderId = GetUserId();
        if (senderId > 0)
        {
            var command = new SendRealtimeNotificationCommand(
                receiverId,
                "message",
                "新訊息",
                message,
                new { SenderId = senderId, Message = message }
            );
            
            await _mediator.Send(command);
        }
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
