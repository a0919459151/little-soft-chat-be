using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.Chat.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Chat.Application.Queries;

public record GetUnreadMessagesQuery(int UserId) : IRequest<List<MessageDto>>;

public class GetUnreadMessagesQueryValidator : AbstractValidator<GetUnreadMessagesQuery>
{
    public GetUnreadMessagesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

public class GetUnreadMessagesQueryHandler : IRequestHandler<GetUnreadMessagesQuery, List<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<GetUnreadMessagesQueryHandler> _logger;

    public GetUnreadMessagesQueryHandler(
        IMessageRepository messageRepository,
        ILogger<GetUnreadMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<List<MessageDto>> Handle(GetUnreadMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _messageRepository.GetUnreadMessagesAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages for user {UserId}", request.UserId);
            return new List<MessageDto>();
        }
    }
}
