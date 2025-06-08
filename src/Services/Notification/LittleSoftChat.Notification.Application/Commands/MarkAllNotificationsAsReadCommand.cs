using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.Application.Commands;

// Command Definition
public record MarkAllNotificationsAsReadCommand(
    int UserId
) : IRequest<bool>;

// Validator
public class MarkAllNotificationsAsReadCommandValidator : AbstractValidator<MarkAllNotificationsAsReadCommand>
{
    public MarkAllNotificationsAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

// Handler
public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<MarkAllNotificationsAsReadCommandHandler> _logger;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationService notificationService,
        ILogger<MarkAllNotificationsAsReadCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.MarkAllAsReadAsync(request.UserId);

            _logger.LogInformation("Marked all notifications as read for user {UserId}", request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", request.UserId);
            return false;
        }
    }
}
