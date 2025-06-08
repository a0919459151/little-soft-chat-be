using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Entities;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.GrpcClients;
using LittleSoftChat.Shared.Infrastructure.HttpClients;

namespace LittleSoftChat.Chat.Application.Commands;

public record SendMessageCommand(int SenderId, int ReceiverId, string Content, string MessageType = "text") : IRequest<SendMessageResult>;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .GreaterThan(0)
            .WithMessage("SenderId must be greater than 0");

        RuleFor(x => x.ReceiverId)
            .GreaterThan(0)
            .WithMessage("ReceiverId must be greater than 0");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content cannot be empty")
            .MaximumLength(2000)
            .WithMessage("Message content cannot exceed 2000 characters");

        RuleFor(x => x.MessageType)
            .NotEmpty()
            .WithMessage("Message type cannot be empty")
            .Must(type => new[] { "text", "image", "file", "emoji" }.Contains(type))
            .WithMessage("Message type must be one of: text, image, file, emoji");

        RuleFor(x => x)
            .Must(command => command.SenderId != command.ReceiverId)
            .WithMessage("Sender and receiver cannot be the same person");
    }
}

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
