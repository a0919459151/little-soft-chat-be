using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Notification.Application.Services;

namespace LittleSoftChat.Notification.Application.Commands;

// Command Definition
public record SendSystemNotificationCommand(
    List<int> UserIds,
    string Title,
    string Content
) : IRequest<bool>;

// Validator
public class SendSystemNotificationCommandValidator : AbstractValidator<SendSystemNotificationCommand>
{
    public SendSystemNotificationCommandValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull()
            .WithMessage("UserIds cannot be null")
            .NotEmpty()
            .WithMessage("UserIds cannot be empty")
            .Must(ids => ids.Count <= 1000)
            .WithMessage("Cannot send system notification to more than 1000 users at once")
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("All UserIds must be greater than 0")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("UserIds cannot contain duplicates");

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
public class SendSystemNotificationCommandHandler : IRequestHandler<SendSystemNotificationCommand, bool>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendSystemNotificationCommandHandler> _logger;

    public SendSystemNotificationCommandHandler(
        INotificationService notificationService,
        ILogger<SendSystemNotificationCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(SendSystemNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.SendSystemNotificationAsync(
                request.UserIds,
                request.Title,
                request.Content);

            _logger.LogInformation("Sent system notification to {UserCount} users", request.UserIds.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system notification to {UserCount} users", request.UserIds.Count);
            return false;
        }
    }
}
