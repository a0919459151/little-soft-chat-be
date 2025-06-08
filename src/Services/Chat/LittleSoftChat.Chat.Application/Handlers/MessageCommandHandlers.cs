using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Chat.Application.Commands;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.HttpClients;

namespace LittleSoftChat.Chat.Application.Handlers;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserGrpcClient _userGrpcClient;
    private readonly INotificationHttpClient _notificationHttpClient;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IMessageRepository messageRepository,
        IUserGrpcClient userGrpcClient,
        INotificationHttpClient notificationHttpClient,
        ILogger<SendMessageCommandHandler> logger)
    {
        _messageRepository = messageRepository;
        _userGrpcClient = userGrpcClient;
        _notificationHttpClient = notificationHttpClient;
        _logger = logger;
    }

    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate users via gRPC
            var sender = await _userGrpcClient.GetUserAsync(request.SenderId);
            var receiver = await _userGrpcClient.GetUserAsync(request.ReceiverId);

            if (sender == null || receiver == null || !sender.IsActive || !receiver.IsActive)
            {
                return SendMessageResult.Failure("Invalid users");
            }

            // 2. Create and save message
            var message = new MessageEntity
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType,
                CreatedAt = DateTime.UtcNow
            };

            var messageId = await _messageRepository.CreateAsync(message);

            // 3. Send notification
            await _notificationHttpClient.SendNotificationAsync(
                request.ReceiverId, 
                $"New message from {sender.DisplayName}", 
                "message"
            );

            _logger.LogInformation("Message {MessageId} sent from {SenderId} to {ReceiverId}", 
                messageId, request.SenderId, request.ReceiverId);

            return SendMessageResult.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {ReceiverId}", 
                request.SenderId, request.ReceiverId);
            return SendMessageResult.Failure("Failed to send message");
        }
    }
}

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, bool>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<MarkMessageAsReadCommandHandler> _logger;

    public MarkMessageAsReadCommandHandler(
        IMessageRepository messageRepository,
        ILogger<MarkMessageAsReadCommandHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _messageRepository.MarkAsReadAsync(request.MessageId, request.UserId);
            
            if (success)
            {
                _logger.LogInformation("Message {MessageId} marked as read by user {UserId}", 
                    request.MessageId, request.UserId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read by user {UserId}", 
                request.MessageId, request.UserId);
            return false;
        }
    }
}
