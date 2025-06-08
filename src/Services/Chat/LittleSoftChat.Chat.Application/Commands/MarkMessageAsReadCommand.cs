using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;

namespace LittleSoftChat.Chat.Application.Commands;

public record MarkMessageAsReadCommand(int MessageId, int UserId) : IRequest<bool>;

public class MarkMessageAsReadCommandValidator : AbstractValidator<MarkMessageAsReadCommand>
{
    public MarkMessageAsReadCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .GreaterThan(0)
            .WithMessage("MessageId must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
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
