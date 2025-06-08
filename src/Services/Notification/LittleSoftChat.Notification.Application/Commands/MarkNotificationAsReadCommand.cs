using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.Application.Commands;

// Command Definition
public record MarkNotificationAsReadCommand(
    int NotificationId,
    int UserId
) : IRequest<bool>;

// Validator
public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .GreaterThan(0)
            .WithMessage("NotificationId must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

// Handler
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

    public MarkNotificationAsReadCommandHandler(
        INotificationService notificationService,
        ILogger<MarkNotificationAsReadCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(request.NotificationId, request.UserId);

            _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", 
                request.NotificationId, request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", 
                request.NotificationId, request.UserId);
            return false;
        }
    }
}
