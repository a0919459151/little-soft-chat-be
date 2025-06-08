using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.Application.Commands;

// Command Definition
public record SendRealtimeNotificationCommand(
    int UserId,
    string Type,
    string Title,
    string Content,
    object? Data = null
) : IRequest<bool>;

// Validator
public class SendRealtimeNotificationCommandValidator : AbstractValidator<SendRealtimeNotificationCommand>
{
    public SendRealtimeNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Notification type is required")
            .MaximumLength(50)
            .WithMessage("Notification type cannot exceed 50 characters")
            .Must(type => new[] { "message", "friend_request", "friend_accepted", "system", "test" }.Contains(type))
            .WithMessage("Invalid notification type");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(1000)
            .WithMessage("Content cannot exceed 1000 characters");
    }
}

// Handler
public class SendRealtimeNotificationCommandHandler : IRequestHandler<SendRealtimeNotificationCommand, bool>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendRealtimeNotificationCommandHandler> _logger;

    public SendRealtimeNotificationCommandHandler(
        INotificationService notificationService,
        ILogger<SendRealtimeNotificationCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(SendRealtimeNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.SendRealtimeNotificationAsync(
                request.UserId,
                request.Type,
                request.Title,
                request.Content,
                request.Data);

            _logger.LogInformation("Sent realtime notification to user {UserId}, type: {Type}", 
                request.UserId, request.Type);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send realtime notification to user {UserId}", request.UserId);
            return false;
        }
    }
}
